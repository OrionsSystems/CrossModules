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
using Orions.Desi.Forms.Core.Util.Extensions;
using Orions.Systems.Desi.Common.TaskExploitation;
using Syncfusion.EJ2.Blazor.Gantt;
using Orions.Desi.Forms.Core.Util.Extensions;
using Orions.SDK.Onvif.DeviceService;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface
{
	public class VideoPlayerBase : BaseComponent
	{
		private NetStore _store;
		private TaskPlaybackInfo _taskPlaybackInfo { get { return CurrentTask.PlaybackInfo; } }
		private DotNetObjectReference<VideoPlayerBase> _componentReference;
		private IFrameCacheService CacheService;
		private TaskCompletionSource<bool> _initializationTaskTcs = new TaskCompletionSource<bool>();
		private List<IDisposable> _subscriptions = new List<IDisposable>();
		private AsyncManualResetEvent _currenVideoSourceLoadedOnClient = new AsyncManualResetEvent(false);

		protected string _videoElementId = $"videoplayer-{Guid.NewGuid().ToString()}";
		private bool _lockPositionUpdate = false;
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
		protected int TotalFrames
		{
			get
			{
				return _taskPlaybackInfo?.TotalFrames ?? 0;
			}
		}
		protected double PlaybackSpeed { get; set; } = 1;
		protected double VolumeLevel { get; set; } = 100;
		protected List<Model.TimelineMarker> TimelineMarkers 
		{ 
			get
			{
				if(_taskPlaybackInfo != null)
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
				else
				{
					return new List<Model.TimelineMarker>();
				}
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
					if (_currentTask != null)
					{
						CurrentPosition = TimeSpan.Zero;
						CurrentFrameIndex = 0;
						UpdateState();
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

					OnMediaInstanceChanged();
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

								GoToSelectedTagFrame();
							}));
						_subscriptions.Add(_tagsStore.Data
							.GetPropertyChangedObservable()
							.Where(i => i.EventArgs.PropertyName == nameof(TagsExploitationData.CurrentTaskTags))
							.Subscribe(_ =>
							{
								UpdateState();
							}));
					}
				});
			}
		}

		private ITaskDataStore _taskDataStore;
		[Parameter]
		public ITaskDataStore TaskDataStore
		{
			get { return _taskDataStore; }
			set
			{
				SetProperty(ref _taskDataStore, value, () =>
				{
					if(value != null)
					{
						_subscriptions.Add(value.CurrentTaskExpandedChanged.Subscribe(_ =>
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

		public async Task GoToNextFrame()
		{
			if (!IsVideoLoading && !CurrentFrameIsLoading && Paused)
			{
				var newFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(this.CurrentPosition) + 1;
				if (newFrameIndex > _taskPlaybackInfo.TotalFrames - 1) newFrameIndex = _taskPlaybackInfo.TotalFrames - 1;

				await GoToFrame(newFrameIndex);
			}
		}

		public async Task GoToPreviousFrame()
		{
			if (!IsVideoLoading && !CurrentFrameIsLoading && Paused)
			{
				var newFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(this.CurrentPosition) - 1;
				if (newFrameIndex < 0) newFrameIndex = 0;

				await GoToFrame(newFrameIndex);
			}
		}

		[JSInvokable]
		public async Task OnVideoEndJs()
		{
			await OnPositionUpdate(_taskPlaybackInfo.TotalDuration.TotalSeconds);
		}

		[JSInvokable]
		public async Task OnPositionUpdate(double positionInSeconds)
		{
			if (_lockPositionUpdate)
			{
				return;
			}

			CurrentPosition = TimeSpan.FromSeconds(positionInSeconds);
			CurrentFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(CurrentPosition);

			if (CurrentPosition >= _taskPlaybackInfo.TotalDuration)
			{
				await Pause();
			}

			if (Paused)
			{
				await UpdateFrameImageByCurrentPosition();
			}
			else
			{
				ActionDispatcher?.Dispatch(UpdatePositionAction.Create(this.MediaInstance, CurrentPosition));
			}

			UpdateState();
		}

		[JSInvokable]
		public async Task OnPlayerDataLoaded()
		{
			_currenVideoSourceLoadedOnClient.Set();
		}

		public async Task OnPlaybackControlPositionChanged(TimeSpan newPosition)
		{
			await OnPositionUpdate(newPosition.TotalSeconds);

			if (!Paused)
			{
				await JSRuntime.InvokeVoidAsync("Orions.Player.setPosition", new object[] { newPosition.TotalSeconds });
			}
		}

		public async Task Play()
		{
			if (!IsVideoLoading && !CurrentFrameIsLoading)
			{
				await _currenVideoSourceLoadedOnClient.WaitAsync();

				ActionDispatcher.Dispatch(SetFrameModeAction.Create(false));

				Paused = false;

				if(CurrentPosition >= TotalDuration)
				{
					CurrentPosition = TimeSpan.Zero;
				}
				
				await JSRuntime.InvokeVoidAsync("Orions.Player.setPositionAndPlay", CurrentPosition.TotalSeconds);
				await OnPlay.InvokeAsync(null);
			}
		}

		public async Task Pause()
		{
			if (!Paused)
			{
				ActionDispatcher.Dispatch(SetFrameModeAction.Create(true));
				Paused = true;
				await JSRuntime.InvokeVoidAsync("Orions.Player.pause", new object[] { });
				await OnPaused.InvokeAsync(null);
			}
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
		}

		protected async override Task OnAfterRenderAsyncSafe(bool firstRender)
		{
			if (firstRender)
			{
				_initializationTaskTcs.SetResult(true);
			}
		}
		#endregion // Component lifecycle hook overrides

		private async Task UpdateFrameImageByCurrentPosition()
		{
			var loaderDelayTask = Task.Delay(300);

			var hyperId = _taskPlaybackInfo.GetPositionHyperId(CurrentFrameIndex);

			if (hyperId.SliceId == null)
				return;

			var frameLoadTask = CacheService.GetCachedFrameAsync(_store, hyperId, null);

			await Task.WhenAny(loaderDelayTask, frameLoadTask);
			if (!frameLoadTask.IsCompleted)
			{
				OnLoading.InvokeAsync(null);
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

			//await JSRuntime.InvokeVoidAsync("Orions.Player.setPosition", new object[] { newPosition.TotalSeconds });
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

		private bool _playerInitialized = false;
		private async Task UpdateCurrentTaskData()
		{
			await _initializationTaskTcs.Task;

			await OnLoading.InvokeAsync(null);
			IsVideoLoading = true;

			var videoDashUrl = (MediaInstance.MediaSource as HyperAssetPlaylistEntry).GetUrl();
			if (!_playerInitialized)
			{
				await JSRuntime.InvokeVoidAsync("Orions.Player.init", _componentReference, _videoElementId, videoDashUrl);
			}
			else
			{
				await JSRuntime.InvokeVoidAsync("Orions.Player.setSrc", videoDashUrl);
			}

			IsVideoLoading = false;
			await OnLoaded.InvokeAsync(null);
			await OnPlay.InvokeAsync(null);
			await Play();

			UpdateState();
		}

		private async Task GoToSelectedTagFrame()
		{
			_lockPositionUpdate = true;

			var tagFrameIdToGoTo = TagsStore?.Data?.SelectedTags?.FirstOrDefault();

			if (tagFrameIdToGoTo != null)
			{
				var frameIndex = _taskPlaybackInfo.GetFrameIndexByHyperId(tagFrameIdToGoTo.TagHyperId);
				await Pause();
				await GoToFrame(frameIndex);
			}

			_lockPositionUpdate = false;
		}

		private async Task OnMediaInstanceChanged()
		{
			UpdateCurrentTaskData();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_subscriptions.ForEach(i => i.Dispose());
				_subscriptions.Clear();
			}

			base.Dispose(disposing);
		}
	}
}
