using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NReco.VideoConverter;
using Orions.Common;
using Orions.Desi.Forms.Core.Services;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Media.Codecs.H264;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.CrossModules.Desi.Services;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Media;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TagsExploitation;
using System.Diagnostics;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface
{
	public class VideoPlayerBase : BaseComponent
	{
		private NetStore _store;
		private TaskPlaybackInfo _taskPlaybackInfo;
		private DotNetObjectReference<VideoPlayerBase> _componentReference;
		private IFrameCacheService CacheService;
		private TaskCompletionSource<bool> _initializationTaskTcs = new TaskCompletionSource<bool>();
		private List<IDisposable> _subscriptions = new List<IDisposable>();
		
		protected string _videoElementId = $"videoplayer-{Guid.NewGuid().ToString()}";
		protected bool CurrentFrameIsLoading { get; set; } = false;
		protected bool IsVideoLoading { get; set; } = true;
		protected bool Paused { get; private set; } = true;
		protected byte[] PayLoad { get; private set; }
		protected string PausedFrameBase64 
		{
			get; set;
		}
		protected int CurrentFrameIndex { get; set; } = 0;
		protected TimeSpan CurrentPosition { get; set; } = TimeSpan.Zero;
		protected TimeSpan TotalDuration
		{
			get
			{
				return _taskPlaybackInfo?.TotalDuration ?? TimeSpan.Zero;
			}
		}
		protected double PlaybackSpeed { get; set; } = 1;
		protected double VolumeLevel { get; set; } = 100;
		protected List<Model.TimelineMarker> TimelineMarkers 
		{ 
			get
			{
				var markers = TagsStore.Data.CurrentTaskTags.GroupBy(t => t.TagHyperId).Select(g => 
					new Model.TimelineMarker
					{
						Id = g.Key.SliceId.ToString(),
						PercentagePosition = _taskPlaybackInfo.GetFrameIndexByHyperId(g.Key) / (double)_taskPlaybackInfo.TotalFrames * 100
					})
					.ToList();

				return markers;
			} 
		}

		public VideoPlayerBase()
		{
			_componentReference = DotNetObjectReference.Create(this);
		}

		#region Parameters
		[Parameter]
		public EventCallback OnPaused { get; set; }

		[Parameter]
		public EventCallback OnPlay { get; set; }

		[Parameter]
		public EventCallback OnLoading { get; set; }

		[Parameter]
		public EventCallback OnLoaded { get; set; }

		private TaskModel _currentTask;
		[Parameter]
		public TaskModel CurrentTask
		{
			get { return _currentTask; }
			set 
			{
				SetProperty(ref _currentTask, value, () =>
				{
					if(_currentTask != null)
					{
						UpdateCurrentTaskData();
					}
				});
			}
		}

		[Parameter]
		public IActionDispatcher ActionDispatcher { get; set; }

		private MediaInstance _mediaInstance;
		[Parameter]
		public MediaInstance MediaInstance
		{
			get { return _mediaInstance; }
			set 
			{
				SetProperty(ref _mediaInstance, value, () =>
				{
					if(_mediaInstance != null)
					{
						_subscriptions.Add(_mediaInstance.GetPropertyChangedObservable()
							.Where(i => i.EventArgs.PropertyName == nameof(MediaInstance.CurrentPosition))
							.Subscribe(_ => UpdateCurrentPositionFrameImage()));
					}
				});
			}
		}

		private ITagsStore _tagsStore;
		[Parameter]
		public ITagsStore TagsStore
		{
			get { return _tagsStore; }
			set 
			{
				SetProperty(ref _tagsStore, value, () =>
				{
					if (_tagsStore != null)
					{
						_subscriptions.Add(_tagsStore.Data
							.SelectedTagsUpdated
							.Subscribe(_ =>
							{
								if (Paused)
								{
									GoToSelectedTagFrame();
								}
							}));
						_subscriptions.Add(_tagsStore.Data
							.CurrentTaskTags
							.GetCollectionChangedObservable()
							.Subscribe(_ =>
							{
								UpdateState();
							}));
					}
				});
			}
		}
		#endregion // Parameters

		[Inject]
		public IKeyboardListener KeyboardListener { get; set; }

		[JSInvokable]
		public async Task OnPauseJsCallback(double positionInSeconds)
		{
			Debug.WriteLine($"Entered {nameof(OnPauseJsCallback)}");

			Paused = true;
			await UpdateFrameImageByCurrentPosition();
			await OnPaused.InvokeAsync(null);
		}

		[JSInvokable]
		public async Task GoToNextFrame()
		{
			if (!IsVideoLoading && !CurrentFrameIsLoading)
			{
				var newFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(this.CurrentPosition) + 1;
				if (newFrameIndex > _taskPlaybackInfo.TotalFrames - 1) newFrameIndex = _taskPlaybackInfo.TotalFrames - 1;

				await GoToFrame(newFrameIndex);
			}
		}

		[JSInvokable]
		public async Task GoToPreviousFrame()
		{
			if (!IsVideoLoading && !CurrentFrameIsLoading)
			{
				var newFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(this.CurrentPosition) - 1;
				if (newFrameIndex < 0) newFrameIndex = 0;

				await GoToFrame(newFrameIndex);
			}
		}

		[JSInvokable]
		public async Task OnPositionUpdate(double positionInSeconds)
		{
			CurrentPosition = TimeSpan.FromMilliseconds(positionInSeconds * 1000);
			CurrentFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(CurrentPosition);

			if(CurrentPosition >= _taskPlaybackInfo.TotalDuration)
			{
				await Pause();
			}

			if (Paused)
			{
				await UpdateFrameImageByCurrentPosition();
			}

			UpdateState();
		}

		public async Task OnPlaybackControlPositionChanged(TimeSpan newPosition)
		{
			await OnPositionUpdate(newPosition.TotalSeconds);

			await JSRuntime.InvokeVoidAsync("Orions.Player.setPosition", new object[] { newPosition.TotalSeconds });
		}

		public async Task Play()
		{
			if (!IsVideoLoading && !CurrentFrameIsLoading)
			{
				Paused = false;
				await JSRuntime.InvokeVoidAsync("Orions.Player.play", new object[] { });
				await OnPlay.InvokeAsync(null);
			}
		}

		public async Task Pause()
		{
			Paused = true;
			await JSRuntime.InvokeVoidAsync("Orions.Player.pause", new object[] { });
		}

		public async Task ChangeVolumeLevel(double value)
		{
			VolumeLevel = value;
			await JSRuntime.InvokeVoidAsync("Orions.Player.setVolumeLevel", new object[] { value });
		}
		
		public async Task ChangePlaybackSpeed(double value)
		{
			PlaybackSpeed = value;
			await JSRuntime.InvokeVoidAsync("Orions.Player.setSpeed", new object[] { value });
		}

		[JSInvokable]
		public async Task OnPlayerDataLoaded()
		{
			await GoToSelectedTagFrame();
		}

		#region Component lifecycle hook overrides
		protected override async Task OnInitializedAsyncSafe()
		{
			await OnLoading.InvokeAsync(null);

			this.CacheService = this.DependencyResolver.GetFrameCacheService();
			this._store = DependencyResolver.GetNetStoreProvider().CurrentNetStore;

			_subscriptions.Add(KeyboardListener.CreateSubscription()
				.AddShortcut(Key.Space, () => { if (Paused) Play(); else Pause(); })
				.AddShortcut(Key.ArrowLeft, () => GoToPreviousFrame())
				.AddShortcut(Key.ArrowRight, () => GoToNextFrame()));

			_initializationTaskTcs.SetResult(true);
		}

		protected async override Task OnAfterRenderAsyncSafe(bool firstRender)
		{
			if (firstRender)
			{
				await JSRuntime.InvokeVoidAsync("Orions.Player.init", _componentReference, _videoElementId);
			}
		}
		#endregion // Component lifecycle hook overrides

		private async Task UpdateFrameImageByCurrentPosition()
		{
			OnLoading.InvokeAsync(null);

			var loaderDelayTask = Task.Delay(300);

			var hyperId = _taskPlaybackInfo.GetPositionHyperId(CurrentFrameIndex);

			if (hyperId.SliceId == null)
				return;

			var frameLoadTask = CacheService.GetCachedFrameAsync(_store, hyperId, null);

			await Task.WhenAny(loaderDelayTask, frameLoadTask);
			if (!frameLoadTask.IsCompleted)
			{
				CurrentFrameIsLoading = true;
				UpdateState();
				await frameLoadTask;
			}

			ActionDispatcher?.Dispatch(UpdatePositionAction.Create(this.MediaInstance, CurrentPosition));

			CurrentFrameIsLoading = false;

			if (!IsVideoLoading)
			{
				OnLoaded.InvokeAsync(null);
			}
			UpdateState();
		}

		private async Task GoToFrame(int index)
		{
			CurrentFrameIndex = index;

			var newPosition = _taskPlaybackInfo.GetPositionByFrameIndex(CurrentFrameIndex);
			CurrentPosition = newPosition;

			await JSRuntime.InvokeVoidAsync("Orions.Player.setPosition", new object[] { newPosition.TotalSeconds });
			await UpdateFrameImageByCurrentPosition();
		}

		private async Task UpdateCurrentPositionFrameImage()
		{
			var frameImage = await CacheService.GetCachedFrameAsync(_store, MediaInstance.CurrentPosition, null);

			if(frameImage != null)
			{
				PausedFrameBase64 = UniImage.ConvertByteArrayToBase64Url(frameImage);
			}
		}

		private async Task UpdateCurrentTaskData()
		{
			await _initializationTaskTcs.Task;
			await OnLoading.InvokeAsync(null);

			IsVideoLoading = true;
			var assetTuple = await InitTaskPlaybackAsync();

			await InitMP4PayloadAsync(assetTuple.track, assetTuple.fragment);
			if (!TagsStore.Data.SelectedTags.Any())
			{
				await UpdateFrameImageByCurrentPosition();
			}

			IsVideoLoading = false;
			Paused = true;
			await OnLoaded.InvokeAsync(null);
			await OnPaused.InvokeAsync(null);
			UpdateState();
		}

		private async Task<(HyperTrack track, HyperFragment fragment)> InitTaskPlaybackAsync()
		{
			var trackArgs = new RetrieveTrackArgs(CurrentTask.HyperId.AssetId.Value, CurrentTask.HyperId.TrackId.Value);
			var track = await _store.ExecuteAsync(trackArgs);

			var fragmentArgs = new RetrieveFragmentArgs(CurrentTask.HyperId.AssetId.Value, CurrentTask.HyperId.TrackId.Value, CurrentTask.HyperId.FragmentId.Value, true);
			var fragment = await _store.ExecuteAsync(fragmentArgs);

			_taskPlaybackInfo = new TaskPlaybackInfo(new HyperFragment[] { fragment }, CurrentTask.HyperId.AssetId.Value, CurrentTask.HyperId.TrackId.Value);

			return (track, fragment);
		}

		private async Task InitMP4PayloadAsync(HyperTrack track, HyperFragment fragment)
		{
			var codecInfo = track.MetaData.Default.GetString(HyperTrackId.VideoCodecPrivateDataFieldName);

			var h264Payload = fragment.SlicesArray.SelectMany((x, i) => i == 0 ? H264Utilities.H264AddCodecDataToPayload(x.Data, codecInfo) : x.Data).ToArray();

			var inputStream = new MemoryStream(h264Payload);

			var ffmpegConverter = new FFMpegConverter();
			var outputStream = new MemoryStream();
			var frameRate = fragment.SliceCount / fragment.Duration.Value.TotalSeconds;
			var settings = new ConvertSettings
			{
				CustomOutputArgs = "-c copy -movflags frag_keyframe+empty_moov -f mp4",
				CustomInputArgs = $"-r {frameRate}"
			};

			var task = ffmpegConverter.ConvertLiveMedia(inputStream, null, outputStream, null, settings);
			task.Start();
			await Task.Run(() => task.Wait());

			PayLoad = outputStream.ToArray();

			await this.JSRuntime.InvokeVoidAsync("Orions.Player.setSrc", PayLoad, _componentReference, new { totalFrames = _taskPlaybackInfo.TotalFrames });
			await this.JSRuntime.InvokeVoidAsync("Orions.Player.setSpeed", PlaybackSpeed);
		}

		private async Task GoToSelectedTagFrame()
		{
			var tagFrameToGoTo = TagsStore?.Data?.SelectedTags?.Min(i => i.TagHyperId.SliceId);
			if (tagFrameToGoTo != null)
			{
				await GoToFrame(tagFrameToGoTo.Value.Index);
				//await OnPaused.InvokeAsync(null);
			}
		}
	}
}
