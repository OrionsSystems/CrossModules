using System;
using System.IO;
using System.Linq;
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
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Media;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TagsExploitation;
using Orions.Systems.Desi.Core.ViewModels;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface
{
	public class VideoPlayerBase : DesiBaseComponent<AuthenticationViewModel>
	{
		private NetStore _store;
		protected TaskPlaybackInfo _taskPlaybackInfo;
		private UniImage _pausedFrame;
		protected DotNetObjectReference<VideoPlayerBase> _componentReference;
		private IFrameCacheService CacheService;
		private TaskCompletionSource<bool> _initializationTaskTcs = new TaskCompletionSource<bool>();
		protected string _videoElementId = $"videoplayer-{Guid.NewGuid().ToString()}";

		public bool CurrentFrameIsLoading { get; set; } = false;
		public bool IsVideoLoading { get; set; } = true;
		protected bool Paused { get; private set; } = true;
		protected byte[] PayLoad { get; private set; }
		protected string PausedFrameBase64 { get { return MediaInstance?.CurrentPositionFrameImage == null ? null : UniImage.ConvertByteArrayToBase64Url(MediaInstance?.CurrentPositionFrameImage); } }

		public VideoPlayerBase()
		{
			_componentReference = DotNetObjectReference.Create(this);
		}

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

		[Parameter]
		public MediaInstance MediaInstance { get; set; }


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
						_tagsStore.Data
							.SelectedTagsUpdated
							.Subscribe(_ =>
							{
								GoToSelectedTagFrame();
							});
					}
				});
			}
		}


		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		public int CurrentFrameIndex { get; set; } = 0;
		public TimeSpan CurrentPosition { get; set; } = TimeSpan.Zero;

		[JSInvokable]
		public async Task OnPauseJsCallback(double positionInSeconds)
		{
			Paused = true;
			await UpdateFrameImageByCurrentPosition();
			await OnPaused.InvokeAsync(null);
		}

		private async Task UpdateFrameImageByCurrentPosition()
		{
			OnLoading.InvokeAsync(null);
			CurrentFrameIsLoading = true;
			UpdateState();

			var hyperId = _taskPlaybackInfo.GetPositionHyperId(CurrentFrameIndex);

			if (hyperId.SliceId == null)
				return;

			// Ask frame cache to load a frame if needed. The CurrentPositionFrameImage will be set automatically on the MediaInstance
			await CacheService.GetCachedFrameAsync(_store, hyperId, null); 
			UpdateState();

			ActionDispatcher?.Dispatch(UpdatePositionAction.Create(this.MediaInstance, CurrentPosition));

			CurrentFrameIsLoading = false;

			if (!IsVideoLoading)
			{
				OnLoaded.InvokeAsync(null);
			}
			UpdateState();
		}

		[JSInvokable]
		public async Task GoToNextFrame()
		{
			var newFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(this.CurrentPosition) + 1;
			if (newFrameIndex > _taskPlaybackInfo.TotalFrames - 1) newFrameIndex = _taskPlaybackInfo.TotalFrames - 1;

			await GoToFrame(newFrameIndex);
		}

		[JSInvokable]
		public async Task GoToPreviousFrame()
		{
			var newFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(this.CurrentPosition) - 1;
			if (newFrameIndex < 0) newFrameIndex = 0;

			await GoToFrame(newFrameIndex);
		}

		[JSInvokable]
		public async Task OnPositionUpdate(double positionInSeconds)
		{
			CurrentPosition = TimeSpan.FromMilliseconds(positionInSeconds * 1000);
			CurrentFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(CurrentPosition);

			if (Paused)
			{
				await UpdateFrameImageByCurrentPosition();
			}
		}

		[JSInvokable]
		public async Task OnPlayJsCallback()
		{
			Paused = false;

			await OnPlay.InvokeAsync(null);
		}

		[JSInvokable]
		public async Task OnPlayerDataLoaded()
		{
			await GoToSelectedTagFrame();
		}

		private async Task GoToFrame(int index)
		{
			CurrentFrameIndex = index;

			var newPosition = _taskPlaybackInfo.GetPositionByFrameIndex(CurrentFrameIndex);
			CurrentPosition = newPosition;

			await UpdateFrameImageByCurrentPosition();
			await JSRuntime.InvokeVoidAsync("Orions.Player.setPosition", new object[] { newPosition.TotalSeconds });
		}

		protected override async Task OnInitializedAsync()
		{
			await OnLoading.InvokeAsync(null);

			this.CacheService = this.DependencyResolver.GetFrameCacheService();
			this._store = DependencyResolver.GetNetStoreProvider().CurrentNetStore;

			_initializationTaskTcs.SetResult(true);
		}

		private async Task UpdateCurrentTaskData()
		{
			await _initializationTaskTcs.Task;
			await OnLoading.InvokeAsync(null);

			IsVideoLoading = true;
			var assetTuple = await InitTaskPlaybackAsync();

			await InitMP4PayloadAsync(assetTuple.track, assetTuple.fragment);

			IsVideoLoading = false;
			await OnLoaded.InvokeAsync(null);
			UpdateState();
		}
		
		protected async override Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);

			if (firstRender)
			{
				await JSRuntime.InvokeVoidAsync("Orions.Player.init", _componentReference, _videoElementId);
			}
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
