using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Common;
using Orions.Desi.Forms.Core.Services;
using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Media;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TagsExploitation;
using Orions.Systems.Desi.Common.TaskExploitation;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.Desi.Common.Util;
using Orions.Systems.CrossModules.Components.Desi.Services;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface
{
	public class VideoPlayerBase : BaseComponent
	{
		private NetStore _store;
		private TaskPlaybackInfo _taskPlaybackInfo { get { return CurrentTask.PlaybackInfo; } }
		private DotNetObjectReference<VideoPlayerBase> _componentReference;
		private TaskCompletionSource<bool> _initializationTaskTcs = new TaskCompletionSource<bool>();
		private List<IDisposable> _subscriptions = new List<IDisposable>();
		private AsyncManualResetEvent _playerReady = new AsyncManualResetEvent(false);
		private AsyncManualResetEvent _playerPauseRequest = new AsyncManualResetEvent(true);
		private AsyncManualResetEvent _playerPlayRequest = new AsyncManualResetEvent(true);
		private PlayerJsState _playerState = PlayerJsState.Initial;

		protected string _videoElementId = $"videoplayer-{Guid.NewGuid().ToString()}";
		private bool _lockPositionUpdate = false;
		protected bool CurrentFrameIsLoading { get; set; } = false;
		protected bool IsVideoLoading { get; set; } = true;
		protected bool IsVideoBuffering { get { return _playerState == PlayerJsState.Buffering; } }
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
				if (_taskPlaybackInfo != null)
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
			get => _currentTask;
			set => SetProperty(ref _currentTask, value, () =>
			{
				if (_currentTask != null)
				{
					CurrentPosition = TimeSpan.Zero;
					CurrentFrameIndex = 0;
				}

				UpdateState();
			});
		}

		[Inject]
		public IActionDispatcher ActionDispatcher { get; set; }

		private MediaInstance _mediaInstance;
		[Parameter]
		public MediaInstance MediaInstance
		{
			get => _mediaInstance;
			set => SetProperty(ref _mediaInstance, value, () =>
			{
				if (_mediaInstance != null)
				{
					_subscriptions.Add(_mediaInstance.GetPropertyChangedObservable()
						.Where(i => i.EventArgs.PropertyName == nameof(MediaInstance.CurrentPositionFrameImage))
						.Select(i => i.Source.CurrentPositionFrameImage)
						.Subscribe(UpdateCurrentPositionFrameImage));
				}
				OnMediaInstanceChanged();
			});
		}

		#endregion // Parameters

		[Inject]
		public ITagsStore TagsStore { get; set; }

		[Inject]
		public ITaskDataStore TaskDataStore { get; set; }

		[Inject]
		public IKeyboardListener KeyboardListener { get; set; }

		[Inject]
		public IFrameCacheService CacheService { get; set; }

		[Inject]
		public INetStoreProvider NetStoreProvider { get; set; }

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
		public async Task OnVideoPlaying()
		{
			_playerState = PlayerJsState.Playing;
			_playerPlayRequest.Set();

			UpdateState();
		}

		[JSInvokable]
		public async Task OnVideoPaused()
		{
			_playerState = PlayerJsState.Paused;
			_playerPauseRequest.Set();

			UpdateState();
		}

		[JSInvokable]
		public async Task OnPlayerReady()
		{
			_playerReady.Set();
			_playerInitialized = true;
			IsVideoLoading = false;
		}

		[JSInvokable]
		public async Task OnVideoEndJs()
		{
			this._playerState = PlayerJsState.Complete;
			await Pause(false);
			await OnPositionUpdate(_taskPlaybackInfo.TotalDuration.TotalSeconds);
		}

		[JSInvokable]
		public async Task OnVideoBuffering(bool buffering, bool isPaused)
		{
			if (buffering)
			{
				this._playerState = PlayerJsState.Buffering;
			}
			else
			{
				this._playerState = isPaused ? PlayerJsState.Paused : PlayerJsState.Playing;
			}

			UpdateState();
		}

		[JSInvokable]
		public async Task OnPositionUpdate(double positionInSeconds)
		{
			await _playerReady.WaitAsync();

			if (_lockPositionUpdate)
			{
				return;
			}

			CurrentPosition = TimeSpan.FromSeconds(positionInSeconds);
			CurrentFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(CurrentPosition);

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
			if (!CurrentFrameIsLoading)
			{
				await _playerReady.WaitAsync();
				ActionDispatcher.Dispatch(SetFrameModeAction.Create(false));
				Paused = false;

				if (CurrentPosition.TotalMillisecondsLong() >= TotalDuration.TotalMillisecondsLong())
				{
					CurrentPosition = TimeSpan.Zero;
				}

				await OnPlay.InvokeAsync(null);
				_playerPlayRequest.Reset();
				await JSRuntime.InvokeVoidAsync("Orions.Player.setPositionAndPlay", CurrentPosition.TotalSeconds);
			}
		}

		public async Task Pause(bool callJsPause = true)
		{
			if (!Paused)
			{
				ActionDispatcher.Dispatch(SetFrameModeAction.Create(true));
				Paused = true;

				if (callJsPause)
				{
					await WaitPlayerApi();
					_playerPauseRequest.Reset();
					await JSRuntime.InvokeVoidAsync("Orions.Player.pause", new object[] { });
					await _playerPauseRequest.WaitAsync();
				}

				await OnPaused.InvokeAsync(null);
			}
		}

		private async Task WaitPlayerApi()
		{
			await _playerReady.WaitAsync();
			await _playerPauseRequest.WaitAsync();
			await _playerPlayRequest.WaitAsync();
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

			_store = NetStoreProvider.CurrentNetStore;

			_subscriptions.Add(KeyboardListener.CreateSubscription()
				.AddShortcut(Key.Space, () => { if (Paused) Play(); else Pause(); })
				.AddShortcut(Key.ArrowLeft, () => GoToPreviousFrame())
				.AddShortcut(Key.ArrowRight, () => GoToNextFrame()));

			_subscriptions.Add(TagsStore.Data.GetPropertyChangedObservable()
				.Where(i => i.EventArgs.PropertyName == nameof(TagsExploitationData.SelectedTags) && i.Source.SelectedTags.IsNotNullOrEmpty())
				.Select(i => i.Source.SelectedTags.Last())
				.Subscribe(i => GoToSelectedTagFrame(i)));

			_subscriptions.Add(TagsStore.Data
				.GetPropertyChangedObservable()
				.Where(i => i.EventArgs.PropertyName == nameof(TagsExploitationData.CurrentTaskTags))
				.Subscribe(_ => UpdateState()));

			_subscriptions.Add(TaskDataStore.CurrentTaskExpandedChanged.Subscribe(_ => UpdateState()));

			if (TagsStore?.Data?.SelectedTags?.LastOrDefault() != null)
			{
				GoToSelectedTagFrame(TagsStore.Data.SelectedTags.Last());
			}
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
			var hyperId = _taskPlaybackInfo.GetPositionHyperId(CurrentFrameIndex);

			if (hyperId.SliceId == null)
				return;

			var frameLoadTask = CacheService.GetCachedFrameAsync(_store, hyperId, null);

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

		private void UpdateCurrentPositionFrameImage(byte[] imageData) => PausedFrameBase64 = imageData != null ? UniImage.ConvertByteArrayToBase64Url(imageData) : null;

		private bool _playerInitialized = false;
		private async Task UpdateCurrentTaskData()
		{
			await _initializationTaskTcs.Task;

			await OnLoading.InvokeAsync(null);

			var videoDashUrl = (MediaInstance.MediaSource as HyperAssetPlaylistEntry).GetUrl();
			if (!_playerInitialized)
			{
				await JSRuntime.InvokeVoidAsync("Orions.Player.init", _componentReference, _videoElementId, videoDashUrl);
			}
			else
			{
				await JSRuntime.InvokeVoidAsync("Orions.Player.setSrc", videoDashUrl);
			}
			IsVideoLoading = true;
			_playerReady.Reset();

			await OnLoaded.InvokeAsync(null);

			if (TagsStore?.Data?.SelectedTags?.Any() ?? false)
			{
				await GoToSelectedTagFrame(TagsStore?.Data?.SelectedTags?.Last());
			}
			else
			{
				await Play();
			}

			UpdateState();
		}

		private async Task GoToSelectedTagFrame(TagModel tag)
		{
			_lockPositionUpdate = true;

			var frameIndex = _taskPlaybackInfo.GetFrameIndexByHyperId(tag.TagHyperId);
			await Pause();
			await GoToFrame(frameIndex);

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

	public static class TimeSpanExtensions
	{
		public static long TotalMillisecondsLong(this TimeSpan timeSpan)
		{
			return (long)timeSpan.TotalMilliseconds;
		}
	}

	public enum PlayerJsState
	{
		Initial = 0,
		Playing = 1,
		Paused = 2,
		Buffering = 3,
		Complete = 4
	}
}

