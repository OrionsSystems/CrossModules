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
using Orions.Node.Common;
using Microsoft.AspNetCore.Components.Web;
using Orions.Systems.Desi.Common.Tracking;
using Orions.Systems.CrossModules.Desi.Components.TaggingSurface.Model;
using System.Threading;

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
		private Task _playerPlayRequest = Task.CompletedTask;
		private PlayerJsState _playerState = PlayerJsState.Initial;

		protected string _videoElementId = $"videoplayer-{Guid.NewGuid().ToString()}";
		private bool _lockPositionUpdate = false;
		protected bool CurrentFrameIsLoading
		{
			get
			{
				if (MediaInstance != null)
				{
					return MediaInstance.CurrentPositionFrameImage == null;
				}
				else
				{
					return false;
				}
			}
		}
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
							Id = g.Key.StringSerializationValue,
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

		protected List<Model.TimelineMarker> TrackingSequenceStartMarkers
		{
			get
			{
				var markers = new List<Model.TimelineMarker>();

				if (TrackingDataStore != null && _taskPlaybackInfo != null)
				{
					var currentTrackingSequenceStart = TrackingDataStore.Data?.TrackingSequenceStart;
					if (currentTrackingSequenceStart != null)
					{
						var currentSequnceStartMarker = new Model.TimelineMarker
						{
							Id = currentTrackingSequenceStart.HyperId.SliceId.ToString(),
							PercentagePosition = _taskPlaybackInfo.GetFrameIndexByHyperId(currentTrackingSequenceStart.HyperId) / (double)_taskPlaybackInfo.TotalFrames * 100
						};

						markers.Add(currentSequnceStartMarker);
					}
				}

				return markers;
			}
		}

		protected List<Model.TimelineMarker> TrackingSequenceEndMarkers
		{
			get
			{
				var markers = new List<Model.TimelineMarker>();

				if (TrackingDataStore?.Data != null && _taskPlaybackInfo != null)
				{
					var currentTrackingSequenceEnd = TrackingDataStore.Data.TrackingSequenceEnd;
					if (currentTrackingSequenceEnd != null)
					{
						var currentSequnceEndMarker = new Model.TimelineMarker
						{
							Id = currentTrackingSequenceEnd.HyperId.SliceId.ToString(),
							PercentagePosition = _taskPlaybackInfo.GetFrameIndexByHyperId(currentTrackingSequenceEnd.HyperId) / (double)_taskPlaybackInfo.TotalFrames * 100
						};

						markers.Add(currentSequnceEndMarker);
					}
				}

				return markers;
			}
		}

		protected List<Model.TimelineMarker> IntermediateElementsMarkers
		{
			get
			{
				var markers = new List<Model.TimelineMarker>();

				if (TrackingDataStore?.Data != null && _taskPlaybackInfo != null)
				{
					foreach(var intermediateElement in TrackingDataStore.Data.IntermediateElements ?? new List<TrackingSequenceElement>())
					{
						var newMarker = new Model.TimelineMarker
						{
							Id = intermediateElement.HyperId.SliceId.ToString(),
							PercentagePosition = _taskPlaybackInfo.GetFrameIndexByHyperId(intermediateElement.HyperId) / (double)_taskPlaybackInfo.TotalFrames * 100
						};
						markers.Add(newMarker);
					}
				}

				return markers;
			}
		}

		protected Tuple<Model.TimelineMarker, Model.TimelineMarker> CurrentSelectedTrackingSequenceBoundaries
		{
			get
			{
				if (TagsStore.Data.SelectedTags.Any(t => t.TrackingSequence != null && t.TrackingSequence.Duration != TimeSpan.Zero))
				{
					var tag = TagsStore.Data.SelectedTags.First(t => t.TrackingSequence != null && t.TrackingSequence.Duration != TimeSpan.Zero);
					var start = new Model.TimelineMarker
					{
						Id = tag.TagHyperId.SliceId.ToString(),
						PercentagePosition = _taskPlaybackInfo.GetFrameIndexByHyperId(tag.TagHyperId) / (double)_taskPlaybackInfo.TotalFrames * 100
					};
					var end = new Model.TimelineMarker
					{
						PercentagePosition = _taskPlaybackInfo.GetFrameIndexByHyperId(tag.TrackingSequence.Elements.Last().HyperId) / (double)_taskPlaybackInfo.TotalFrames * 100
					};

					return new Tuple<Model.TimelineMarker, Model.TimelineMarker>(start, end);
				}

				return null;
			}
		}

		public VideoPlayerBase()
		{
			_componentReference = DotNetObjectReference.Create(this);
		}

		#region Parameters
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
					//_subscriptions.Add(_mediaInstance.GetPropertyChangedObservable()
					//	.Where(i => i.EventArgs.PropertyName == nameof(MediaInstance.CurrentPositionFrameImage))
					//	.Select(i => i.Source.CurrentPositionFrameImage)
					//	.Subscribe(UpdateCurrentPositionFrameImage));
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

		[Inject]
		public IMediaDataStore MediaDataStore { get; set; }

		[Inject]
		public ITrackingDataStore TrackingDataStore { get; set; }

		public async Task GoToNextFrame()
		{
			if (Paused)
			{
				var newFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(this.CurrentPosition) + 1;
				if (newFrameIndex > _taskPlaybackInfo.TotalFrames - 1) newFrameIndex = _taskPlaybackInfo.TotalFrames - 1;

				await GoToFrame(newFrameIndex);
			}
		}

		public async Task GoToPreviousFrame()
		{
			if (Paused)
			{
				var newFrameIndex = _taskPlaybackInfo.GetFrameIndexByPosition(this.CurrentPosition) - 1;
				if (newFrameIndex < 0) newFrameIndex = 0;

				await GoToFrame(newFrameIndex);
			}
		}

		[JSInvokable]
		public async Task OnVideoPaused()
		{
			_playerState = PlayerJsState.Paused;

			UpdateState();
		}

		[JSInvokable]
		public async Task OnPlayerReady()
		{
			_playerReady.Set();
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

			ActionDispatcher?.Dispatch(UpdatePositionAction.Create(this.MediaInstance, CurrentPosition));

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
			try
			{
				await WaitPlayerApi();
				ActionDispatcher.Dispatch(SetFrameModeAction.Create(false));
				Paused = false;

				if (CurrentPosition.TotalMillisecondsLong() >= TotalDuration.TotalMillisecondsLong())
				{
					CurrentPosition = TimeSpan.Zero;
				}

				_playerPlayRequest = JSRuntime.InvokeVoidAsyncWithPromise("Orions.Player.setPositionAndPlay", CurrentPosition.TotalSeconds);
				await _playerPlayRequest;
				_playerState = PlayerJsState.Playing;
			}
			catch(Exception e)
			{
				Logger?.LogException("An exception occured while trying to start a playback", e);
				throw;
			}
		}

		public async Task Pause(bool callJsPause = true)
		{
			if (!Paused)
			{
				if (callJsPause)
				{
					await WaitPlayerApi();
					await JSRuntime.InvokeVoidAsyncWithPromise("Orions.Player.pause");
				}

				Paused = true;
				ActionDispatcher.Dispatch(SetFrameModeAction.Create(true));

				UpdateState();
			}
		}

		private async Task WaitPlayerApi()
		{
			await _playerReady.WaitAsync();
			await _playerPlayRequest;
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
			_store = NetStoreProvider.CurrentNetStore;

			_subscriptions.Add(KeyboardListener.CreateSubscription()
				.AddShortcut(Key.Space, () => { if (Paused) Play(); else Pause(); })
				.AddShortcut(Key.ArrowLeft, () => GoToPreviousFrame())
				.AddShortcut(Key.ArrowRight, () => GoToNextFrame()));

			_subscriptions.Add(TagsStore.Data.GetPropertyChangedObservable()
				.Where(i => i.EventArgs.PropertyName == nameof(TagsExploitationData.SelectedTags) && i.Source.SelectedTags.IsNotNullOrEmpty())
				.Select(i => i.Source.SelectedTags.Last())
				.Subscribe(i => GoToSelectedTagFrame(i)));
			_subscriptions.Add(TagsStore.Data.GetPropertyChangedObservable()
				.Where(i => i.EventArgs.PropertyName == nameof(TagsExploitationData.SelectedTags))
				.Subscribe(_ => UpdateState()));

			_subscriptions.Add(TagsStore.Data
				.GetPropertyChangedObservable()
				.Where(i => i.EventArgs.PropertyName == nameof(TagsExploitationData.CurrentTaskTags))
				.Subscribe(_ => UpdateState()));

			_subscriptions.Add(TaskDataStore.CurrentTaskExpandedChanged.Subscribe(_ => UpdateState()));
			_subscriptions.Add(MediaDataStore.FrameImageChanged.Subscribe(_ => UpdateState()));
			_subscriptions.Add(
				TrackingDataStore.Data.GetPropertyChangedObservable()
					.Where(i => i.EventArgs.PropertyName == nameof(TrackingData.TrackingSequenceStart)
						|| i.EventArgs.PropertyName == nameof(TrackingData.TrackingSequenceEnd))
					.Subscribe(_ => UpdateState()));

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

		private async Task GoToFrame(int index)
		{
			CurrentFrameIndex = index;

			var newPosition = _taskPlaybackInfo.GetPositionByFrameIndex(CurrentFrameIndex);
			CurrentPosition = newPosition;

			ActionDispatcher?.Dispatch(UpdatePositionAction.Create(this.MediaInstance, CurrentPosition));

			UpdateState();
		}

		private bool _playerInitialized = false;
		private async Task UpdateCurrentTaskData()
		{
			await _initializationTaskTcs.Task;

			var videoDashUrl = (MediaInstance.MediaSource as HyperAssetPlaylistEntry).GetUrl();
			IsVideoLoading = true;
			_playerReady.Reset();
			_lockPositionUpdate = true; // lock position update to prevent initial timeupdate after loading new video source
			if (!_playerInitialized)
			{
				await JSRuntime.InvokeVoidAsyncWithPromise("Orions.Player.init", _componentReference, _videoElementId, videoDashUrl);
			}
			else
			{
				await JSRuntime.InvokeVoidAsyncWithPromise("Orions.Player.setSrc", videoDashUrl);
			}
			_playerInitialized = true;

			if (TagsStore?.Data?.SelectedTags?.Any() ?? false)
			{
				await GoToSelectedTagFrame(TagsStore?.Data?.SelectedTags?.Last());
			}
			else
			{
				await Play();
			}

			_lockPositionUpdate = false;

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

		protected void OnTimelineTagMarkerClicked(TimelineMarker marker)
		{
			var tagId = HyperId.Parse(marker.Id);
			var tag = TagsStore.Data.CurrentTaskTags.SingleOrDefault(t => t.TagHyperId.Equals(tagId));

			ActionDispatcher.Dispatch(SelectSingleTagAction.Create(tag));
		}

		[JSInvokable]
		public async Task OnMouseWheelHandler(bool up)
		{
			if(up)
			{
				GoToPreviousFrame();
			}
			else
			{
				GoToNextFrame();
			}

			UpdateState();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_subscriptions.ForEach(i => i.Dispose());
				_subscriptions.Clear();
			}

			JSRuntime.InvokeVoidAsync("Orions.Player.dispose");

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

