using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Media;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TagsExploitation;
using Orions.Systems.Desi.Common.TaskExploitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rectangle = Orions.Systems.CrossModules.Desi.Components.TaggingSurface.Model.Rectangle;
using System.Reactive.Concurrency;
using Orions.Common;
using System.Diagnostics;
using Orions.Systems.CrossModules.Components.Desi.Services;
using Orions.Systems.Desi.Common.Util;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;
using System.Runtime.CompilerServices;
using Orions.Systems.CrossModules.Desi.Components.TaggingSurface.Model;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface
{
	public class TaggingSurfaceBase : BaseComponent
	{
		private const int UpdateRateMilliseconds = 200;

		private readonly string TaggedSelectedMarkerBoundsColor = "#0000FF";
		private readonly string UntaggedSelectedMarkerBoundsColor = "#FF0000";
		private readonly string TaggedUnselectedMarkerBoundsColor = "#E3FF00";
		private readonly string UntaggedUnselectedMarkerBoundsColor = "#FFFF00";
		private readonly string TrackingSequenceElementMarkerBoundsColor = "#FF0000";

		private string _componentId { get; set; }
		private List<Rectangle> _rectangles = new List<Rectangle>();
		private byte[] _lastFrameRendered;
		private SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(1, 1);
		private SemaphoreSlim _updateTagsClientSemaphore = new SemaphoreSlim(1, 1);
		private TaskCompletionSource<bool> _initializationTaskTcs = new TaskCompletionSource<bool>();
		private DotNetObjectReference<TaggingSurfaceBase> _componentJsReference { get; set; }
		private AsyncManualResetEvent _currentPositionFrameRendered = new AsyncManualResetEvent(false);
		private AsyncManualResetEvent _rectanglesUpdated = new AsyncManualResetEvent(true);

		private bool _showFrameLoadOverlay = true;
		public bool CurrentFrameIsRendering
		{
			get
			{
				return (MediaDataStore?.Data?.FrameModeEnabled ?? false) && _showFrameLoadOverlay;
			}
		}

		[Parameter]
		public EventCallback<TagModel> OnTagSelected { get; set; }

		[Inject]
		public IKeyboardListener KeyboardListener { get; set; }

		[Inject]
		public IMediaDataStore MediaDataStore { get; set; }

		[Inject]
		public ITaskDataStore TaskDataStore { get; set; }

		[Inject]
		public ITagsStore TagsStore { get; set; }

		[Inject]
		public IActionDispatcher ActionDispatcher { get; set; }

		private object _rectanglesSetterLock = new object();
		private List<Rectangle> Rectangles
		{
			get => _rectangles;
			set
			{
				lock (_rectanglesSetterLock)
				{
					QueueUpdateTagsOnClient(value);
				}
			}
		}

		private RenderQueueHelper<List<Rectangle>> _tagsClientUpdateQueue = new RenderQueueHelper<List<Rectangle>>();
		private void QueueUpdateTagsOnClient(List<Rectangle> newCollection)
		{
			_tagsClientUpdateQueue.Enqueue(UpdateTagsOnClient, newCollection);
		}

		[Parameter]
		public RenderFragment ChildContent { get; set; }

		protected string ComponentIdHtml { get { return $"tagging-surface-{this._componentId}"; } }

		public TaggingSurfaceBase()
		{
			_componentId = Guid.NewGuid().ToString();
			_componentJsReference = DotNetObjectReference.Create<TaggingSurfaceBase>(this);
		}

		[JSInvokable]
		public async Task<string> TagAdded(Rectangle rectangle)
		{
			rectangle.Id = Guid.NewGuid().ToString();

			OnGeometryCreated(rectangle);

			return rectangle.Id;
		}

		[JSInvokable]
		public async Task TagSelected(string id)
		{
			OnTagSelectionToggled(id);
		}

		public async Task AttachElementPositionToRectangle(string rectangleId, string elementSelector)
		{
			await _initializationTaskTcs.Task;
			await _currentPositionFrameRendered.WaitAsync();
			if (this.Rectangles.Any(r => r.Id == rectangleId))
			{
				await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.attachElementPositionToTag", new object[] { _componentId, rectangleId, elementSelector });
			}
		}

		[JSInvokable]
		public async Task TagPositionOrSizeChanged(Rectangle rectangle) => OnTagPositionOrSizeChanged(rectangle);

		private RenderQueueHelper<byte[]> _renderQueueHelper = new RenderQueueHelper<byte[]>();
		private async Task OnCurrentPositionFrameImageChanged()
		{
			await _initializationTaskTcs.Task;

			_renderQueueHelper.Enqueue(RenderFrame, MediaDataStore?.Data?.MediaInstances?.FirstOrDefault()?.CurrentPositionFrameImage);
		}

		public async Task RenderFrame(byte[] newFrameImage)
		{
			await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.resetZoom", new object[] { _componentId });

			if (!EqualityComparer<byte[]>.Default.Equals(_lastFrameRendered, newFrameImage) && newFrameImage != null)
			{
				await _rectanglesUpdated.WaitAsync();
				_currentPositionFrameRendered.Reset();
				_lastFrameRendered = newFrameImage;

				await JSRuntime.InvokeVoidAsyncWithPromise("Orions.TaggingSurface.updateFrameImage", new object[] { _componentId, UniImage.ConvertByteArrayToBase64Url(newFrameImage) });

				_currentPositionFrameRendered.Set();
			}
			else if (_lastFrameRendered != null)
			{
				_currentPositionFrameRendered.Set();
			}

			UpdateState();
			UpdateRectangles();
		}

		protected override void OnInitializedSafe()
		{
			base.OnInitializedSafe();

			_dataStoreSubscriptions.Add(KeyboardListener.CreateSubscription()
				.AddShortcut(Key.Shift, KeyModifiers.Shift, () => SetSelectionMode(TagsSelectionMode.Multiple), KeyboardEventType.KeyDown)
				.AddShortcut(Key.Shift, KeyModifiers.None, () => SetSelectionMode(TagsSelectionMode.Single), KeyboardEventType.KeyUp));

			_dataStoreSubscriptions.Add(MediaDataStore.PositionUpdated.Subscribe(_ =>
			{
				UpdateState();
				UpdateRectangles();
			}));
			_dataStoreSubscriptions.Add(MediaDataStore.FrameImageChanged.Subscribe(_ => OnCurrentPositionFrameImageChanged()));

			_dataStoreSubscriptions.Add(MediaDataStore.Data.GetPropertyChangedObservable()
				.Where(i => i.EventArgs.PropertyName == nameof(MediaData.MediaInstances) && i.Source.MediaInstances.IsNotNullOrEmpty())
				.Subscribe(_ =>
				{
					_currentPositionFrameRendered.Reset();
					OnCurrentPositionFrameImageChanged();
				}));

			if (MediaDataStore?.Data?.MediaInstances?.FirstOrDefault() != null)
			{
				_currentPositionFrameRendered.Reset();
				OnCurrentPositionFrameImageChanged();
				UpdateRectangles();
			};

			_dataStoreSubscriptions.Add(MediaDataStore.Data.GetPropertyChangedObservable()
				.Where(i => i.EventArgs.PropertyName == nameof(MediaData.DefaultMediaInstance))
				.Subscribe(_ => OnDefaultMediaInstanceChanged()));

			_dataStoreSubscriptions.Add(TaskDataStore.CurrentTaskChanged.Subscribe(_ => OnCurrentTaskChanged()));

			_dataStoreSubscriptions.Add(MediaDataStore.FrameModeChanged.Subscribe(mediaData => OnFrameModeChanged(mediaData.FrameModeEnabled)));

			var aggregatedSub = Observable.Merge(
				TagsStore.CurrentTaskTagsCollectionChanged.Select(_ => true),
				TagsStore.TagPropertyChanged
					.Where(i => CommonExtensions.IsOneOf(i.PropertyName, nameof(TagModel.Geometry), nameof(TagModel.IsSelected), nameof(TagModel.LabelValue)))
					.Select(_ => true))
				.Sample(TimeSpan.FromMilliseconds(UpdateRateMilliseconds))
				.ObserveOn(Scheduler.Default)
				.Subscribe(_ => UpdateRectangles());

			_dataStoreSubscriptions.AddItem(aggregatedSub);

			_renderQueueHelper.WaitCallbackDelay = 300;
			_renderQueueHelper.WaitStartCallback = () =>
			{
				_showFrameLoadOverlay = true;
				UpdateState();
			};
			_renderQueueHelper.WaitCompleteCallback = () =>
			{
				_showFrameLoadOverlay = MediaDataStore.Data.DefaultMediaInstance.CurrentPositionFrameImage == null;
				UpdateState();
			};

			UpdateRectangles();
		}

		protected override async Task OnAfterRenderAsyncSafe(bool firstRender)
		{
			await _initializationSemaphore.WaitAsync();
			if (!_initializationTaskTcs.Task.IsCompleted && MediaDataStore.Data.MediaInstances?.Any() == true)
			{
				await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.setupTaggingSurface", new object[] { _componentJsReference, _componentId });
				_initializationTaskTcs.SetResult(true);
				await OnFrameModeChanged(MediaDataStore.Data.FrameModeEnabled);
			}
			_initializationSemaphore.Release();

		}

		public void OnTagPositionOrSizeChanged(Rectangle rectangle)
		{
			var rectF = new System.Drawing.RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			var tagModel = TagsStore.Data.CurrentTaskTags.SingleOrDefault(t => t.Id.ToString() == rectangle.Id);

			if (tagModel != null)
			{
				tagModel.Geometry = tagModel.Geometry.WithNewBounds(rectF);
			}
		}

		private async Task OnFrameModeChanged(bool frameModeEnabled)
		{
			UpdateState();
			await _initializationTaskTcs.Task;
			await this.JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.setFrameMode", _componentId, frameModeEnabled);
		}

		private async Task OnCurrentTaskChanged()
		{
			await _currentPositionFrameRendered.WaitAsync();

			UpdateRectangles();
		}

		private void UpdateRectangles()
		{
			Rectangles = CurrentPositionRectanglesSelector(TagsStore.Data.CurrentTaskTags.ToList() ?? Enumerable.Empty<TagModel>());
		}

		protected List<Rectangle> CurrentPositionRectanglesSelector(IEnumerable<TagModel> tags)
		{
			if (TaskDataStore.Data.CurrentTask != null)
			{
				if (TaskDataStore.Data.CurrentTask.ContentMode == ContentModes.Video)
				{
					var position = GetCurrentPosition();
					var tagRectangles = new List<Rectangle>(tags.Where(t => t.TagHyperId.Equals(position)).Select(ConvertToRectangle));
					var trackingRectangles = new List<Rectangle>(
						tags.Where(t => t.IsSelected && (t.TrackingSequence?.Elements?.Any(e => e.HyperId.Equals(position)) ?? false)).SelectMany(t => t.TrackingSequence.Elements.Where(e => e.HyperId.Equals(position))).Select(ConvertToTrackingSequenceRectangle));

					return tagRectangles.Concat(trackingRectangles).ToList();
				}
				else if (TaskDataStore.Data.CurrentTask.ContentMode == ContentModes.Image || TaskDataStore.Data.CurrentTask.IsComparative())
				{
					return new List<Rectangle>(tags.Select(ConvertToRectangle));
				}
			}

			return new List<Rectangle>();
		}

		private Rectangle ConvertToTrackingSequenceRectangle(TrackingSequenceElement sequenceElement)
		{
			var rectangle = new Rectangle
			{
				X = sequenceElement.Geometry.ProportionalBounds.X,
				Y = sequenceElement.Geometry.ProportionalBounds.Y,
				Height = sequenceElement.Geometry.ProportionalBounds.Height,
				Width = sequenceElement.Geometry.ProportionalBounds.Width,
				Id = sequenceElement.HyperId.ToString(),
				BorderColor = TrackingSequenceElementMarkerBoundsColor,
				IsSelected = false,
				Label = "",
				BorderType = Rectangle.BorderTypeEnum.Dashed,
				IsReadonly = true
			};

			return rectangle;
		}

		private Rectangle ConvertToRectangle(TagModel tagModel)
		{
			var rect = new Rectangle
			{
				X = tagModel.Geometry.ProportionalBounds.X,
				Y = tagModel.Geometry.ProportionalBounds.Y,
				Height = tagModel.Geometry.ProportionalBounds.Height,
				Width = tagModel.Geometry.ProportionalBounds.Width,
				Id = tagModel.Id.ToString(),
				IsSelected = tagModel.IsSelected,
				Label = tagModel.IsSelected ? "" : tagModel.LabelValue
			};

			if (tagModel.TagonomyExecutionResult != null)
			{
				rect.BorderColor = tagModel.IsSelected ? TaggedSelectedMarkerBoundsColor : TaggedUnselectedMarkerBoundsColor;
			}
			else
			{
				rect.BorderColor = tagModel.IsSelected ? UntaggedSelectedMarkerBoundsColor : UntaggedUnselectedMarkerBoundsColor;
			}

			return rect;
		}

		private void OnGeometryCreated(Rectangle geometry)
		{
			var rectF = new System.Drawing.RectangleF(geometry.X, geometry.Y, geometry.Width, geometry.Height);
			var tagsGeometry = new TagGeometry(rectF, ShapeType.Rectangle);
			ActionDispatcher.Dispatch(CreateNewTagAction.Create(tagsGeometry, GetCurrentPosition()));
		}

		public void OnTagSelectionToggled(string id)
		{
			var tagModel = TagsStore.Data.CurrentTaskTags.Single(t => t.Id.ToString() == id);
			ActionDispatcher.Dispatch(ToggleTagSelectionAction.Create(tagModel));
		}

		private HyperId GetCurrentPosition()
		{
			if (MediaDataStore != null && MediaDataStore.Data.MediaInstances.Count == 1)
			{
				return MediaDataStore.Data.MediaInstances[0].CurrentPosition;
			}

			return TaskDataStore.Data.CurrentTask?.HyperId ?? new HyperId();
		}

		public static TaggingSurfaceBase CurrentTaggingSurface;

		private async Task UpdateTagsOnClient(List<Rectangle> newRectangleCollection)
		{
			try
			{
				await Task.WhenAll(_initializationTaskTcs.Task, _currentPositionFrameRendered.WaitAsync());
				_rectanglesUpdated.Reset();

				try
				{
					await _updateTagsClientSemaphore.WaitAsync();

					var oldRectangleCollection = _rectangles;

					var rectanglesUpdateRequest = new RectanglesUpdateRequest();
					// update tags
					foreach (var newTag in newRectangleCollection)
					{
						var oldTag = oldRectangleCollection.SingleOrDefault(t => t.Id == newTag.Id);
						if (oldTag != null && !oldTag.Equals(newTag))
						{
							rectanglesUpdateRequest.Updates.Add(newTag);
							//await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.updateTag", new object[] { _componentId, newTag });
						}
					}

					// remove removed tags
					foreach (var oldTag in oldRectangleCollection)
					{
						if (newRectangleCollection.Any(t => t.Id == oldTag.Id))
						{
							continue;
						}
						else
						{
							rectanglesUpdateRequest.Removals.Add(oldTag);
							//await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.removeTag", new object[] { _componentId, oldTag });
						}
					}

					// add newly added tags
					foreach (var newTag in newRectangleCollection)
					{
						if (oldRectangleCollection.Any(t => t.Id == newTag.Id))
						{
							continue;
						}
						else
						{
							rectanglesUpdateRequest.Addings.Add(newTag);
							//await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.addTag", new object[] { _componentId, newTag });
						}
					}

					await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.updateRectangles", new object[] { _componentId, rectanglesUpdateRequest });

					_rectangles = newRectangleCollection;
				}
				catch (Exception e)
				{
					Logger.LogException("Exception occured while trying to update tags on the canvas", e);
					throw;
				}
				finally
				{
					_updateTagsClientSemaphore.Release();
				}
			}
			finally
			{
				_rectanglesUpdated.Set();
			}
		}

		private void SetSelectionMode(TagsSelectionMode mode)
		{
			ActionDispatcher.Dispatch(SetTagsSelectionModeAction.Create(mode));
		}

		private async Task OnDefaultMediaInstanceChanged()
		{
			UpdateRectangles();
			UpdateState();
		}

		protected override void Dispose(bool disposing)
		{
			JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.dispose", new object[] { _componentId });

			base.Dispose(disposing);
		}
	}

	public class RenderQueueHelper<T>
	{
		private List<(Func<T, Task>, T args)> _queue = new List<(Func<T, Task>, T args)>();
		private object _operationsLock = new object();
		private bool _queueIsRunning = false;
		public Action WaitStartCallback { get; set; }
		public Action WaitCompleteCallback { get; set; }
		public int WaitCallbackDelay { get; set; } = 100;

		public void SetRenderWaitCallback(Action start, Action complete, int delay)
		{
			WaitStartCallback = start;
			WaitCompleteCallback = complete;
			WaitCallbackDelay = 200;
		}

		public void Enqueue(Func<T, Task> action, T args)
		{
			lock (_operationsLock)
			{
				if (_queue.Count == 0)
				{
					_queue.Add((action, args));
					RunQueue();
				}
				else if (_queue.Count == 1)
				{
					_queue.Add((action, args));
				}
				else
				{
					_queue.RemoveAt(1);
					_queue.Add((action, args));
				}
			}
		}

		private async Task RunQueue()
		{
			_queueIsRunning = true;
			RunWaitStartCallback();
			while (true)
			{
				(Func<T, Task>, T) item;
				lock (_operationsLock)
				{
					var nextExists = _queue.Any();
					if (!nextExists)
					{
						break;
					}
					else
					{
						item = _queue.First();
					}
				}

				await item.Item1.Invoke(item.Item2);

				lock (_operationsLock)
				{
					_queue.RemoveAt(0);
				}
			}
			RunWaitCompleteCallback();
			_queueIsRunning = false;
		}

		private void RunWaitCompleteCallback()
		{
			WaitCompleteCallback?.Invoke();
		}

		private async Task RunWaitStartCallback()
		{
			await Task.Delay(WaitCallbackDelay);
			if (_queueIsRunning)
			{
				WaitStartCallback?.Invoke();
			}
		}
	}

	public static class EnumerableDebugExtensions
	{
		public static string ToListDebug(this IEnumerable<Rectangle> collection)
		{
			return string.Join(',', collection.Select(r => r.Id));
		}
	}
}
