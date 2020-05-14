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
using Wintellect.PowerCollections;
using Orions.Common;
using Orions.Systems.CrossModules.Desi.Services;
using System.Workflow.ComponentModelEx;
using Syncfusion.EJ2.Blazor.Charts;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface
{
	public class TaggingSurfaceBase : BaseComponent
	{
		private const int UpdateRateMilliseconds = 50;

		private readonly string TaggedSelectedMarkerBoundsColor = "#0000FF";
		private readonly string UntaggedSelectedMarkerBoundsColor = "#FF0000";
		private readonly string TaggedUnselectedMarkerBoundsColor = "#E3FF00";
		private readonly string UntaggedUnselectedMarkerBoundsColor = "#FFFF00";

		private string _componentId { get; set; }
		private List<Rectangle> _rectangles = new List<Rectangle>();
		private IMediaDataStore _mediaDataStore;
		private ITagsStore _tagsStore;
		private ITaskDataStore _taskDataStore;
		private byte[] _lastFrameRendered;
		private SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(1, 1);
		private SemaphoreSlim _updateTagsClientSemaphore = new SemaphoreSlim(1, 1);
		private TaskCompletionSource<bool> _initializationTaskTcs = new TaskCompletionSource<bool>();
		protected bool _mediaPaused = true;
		private DotNetObjectReference<TaggingSurfaceBase> _componentJsReference { get; set; }
		private AsyncManualResetEvent _currentPositionFrameRendered = new AsyncManualResetEvent(false);

		private IKeyboardListener _keyboardListener;
		[Inject]
		public IKeyboardListener KeyboardListener
		{
			get { return _keyboardListener; }
			set
			{
				SetProperty(ref _keyboardListener, value, () =>
				{
					_dataStoreSubscriptions.Add(KeyboardListener.CreateSubscription()
						.AddShortcut(Key.Shift, KeyModifiers.Shift, () => SetSelectionMode(TagsSelectionMode.Multiple), KeyboardEventType.KeyDown)
						.AddShortcut(Key.Shift, KeyModifiers.None, () => SetSelectionMode(TagsSelectionMode.Single), KeyboardEventType.KeyUp)
						);
				});
			}
		}

		[Parameter]
		public IMediaDataStore MediaDataStore
		{
			get => _mediaDataStore;
			set => SetProperty(ref _mediaDataStore,
				value,
				() =>
				{
					_dataStoreSubscriptions.Add(
						value.Data.GetPropertyChangedObservable().Where(i => i.EventArgs.PropertyName == nameof(MediaData.MediaInstances)).Subscribe(_ =>
						{
							if (value?.Data?.MediaInstances?.FirstOrDefault()?.CurrentPosition != null)
							{
								_currentPositionFrameRendered.Reset();

								OnCurrentPositionFrameImageChanged();
							};

							UpdateRectangles();

							if (value?.Data?.MediaInstances != null)
							{
								_dataStoreSubscriptions.Add(value.Data.MediaInstances[0].GetPropertyChangedObservable().Where(i => i.EventArgs.PropertyName == nameof(MediaInstance.CurrentPosition)).Subscribe(_ =>
								{
									_currentPositionFrameRendered.Reset();
									UpdateRectangles();
								}));

								_dataStoreSubscriptions.Add(
									value.Data.MediaInstances[0].GetPropertyChangedObservable().Where(i => i.EventArgs.PropertyName == nameof(MediaInstance.CurrentPositionFrameImage)).Subscribe(_ =>
									{
										OnCurrentPositionFrameImageChanged();
									}));
							}
						}));

					_dataStoreSubscriptions.Add(value.Data.MediaInstances[0].GetPropertyChangedObservable().Where(i => i.EventArgs.PropertyName == nameof(MediaInstance.CurrentPosition)).Subscribe(_ =>
						{
							_currentPositionFrameRendered.Reset();
							UpdateRectangles();
						}));

					_dataStoreSubscriptions.Add(
						value.Data.MediaInstances[0].GetPropertyChangedObservable().Where(i => i.EventArgs.PropertyName == nameof(MediaInstance.CurrentPositionFrameImage)).Subscribe(_ =>
						{
							OnCurrentPositionFrameImageChanged();
						}));

					if (value?.Data?.MediaInstances?.FirstOrDefault()?.CurrentPosition != null)
					{
						_currentPositionFrameRendered.Reset();
						OnCurrentPositionFrameImageChanged();
					};
				});
		}

		[Parameter]
		public ITagsStore TagsStore
		{
			get => _tagsStore;
			set => SetProperty(ref _tagsStore,
				value,
				() =>
				{
					var aggregatedSub = Observable.Merge(
						value.CurrentTaskTagsCollectionChanged.Select(_ => true),
						value.TagPropertyChanged
							.Where(i => CommonExtensions.IsOneOf(i.PropertyName, nameof(TagModel.Geometry), nameof(TagModel.IsSelected), nameof(TagModel.LabelValue)))
							.Select(_ => true))
					.Sample(TimeSpan.FromMilliseconds(UpdateRateMilliseconds))
					.ObserveOn(Scheduler.Default)
					.Subscribe(_ => UpdateRectangles());

					_dataStoreSubscriptions.AddItem(aggregatedSub);

					UpdateRectangles();
				});
		}

		[Parameter]
		public ITaskDataStore TaskDataStore
		{
			get => _taskDataStore;
			set => SetProperty(ref _taskDataStore, value, () => _taskDataStore?.CurrentTaskChanged.Subscribe(_ => OnCurrentTaskChanged()));
		}

		[Parameter]
		public IActionDispatcher ActionDispatcher { get; set; }

		private object _rectanglesSetterLock = new object();

		private List<Rectangle> Rectangles
		{
			get => _rectangles;
			set
			{
				lock (_rectanglesSetterLock)
				{
					UpdateTagsOnClient(_rectangles, value);
					_rectangles = value;
				}
			}
		}

		[Parameter]
		public RenderFragment ChildContent { get; set; }

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

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

		[JSInvokable]
		public async Task FrameImageRendered()
		{
			_lastFrameRendered = MediaDataStore?.Data?.MediaInstances[0].CurrentPositionFrameImage;
			_currentPositionFrameRendered.Set();
		}

		private async Task OnCurrentPositionFrameImageChanged()
		{
			await _initializationTaskTcs.Task;
			await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.resetZoom", new object[] { _componentId });

			var newFrameImage = MediaDataStore?.Data?.MediaInstances?.FirstOrDefault()?.CurrentPositionFrameImage;

			if (!EqualityComparer<byte[]>.Default.Equals(_lastFrameRendered, newFrameImage)
				&& newFrameImage != null)
			{
				await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.updateFrameImage", new object[] { _componentId, UniImage.ConvertByteArrayToBase64Url(newFrameImage) });
			}
			else
			{
				_currentPositionFrameRendered.Set();
			}

			UpdateRectangles();
		}

		protected override async Task OnAfterRenderAsyncSafe(bool firstRender)
		{
			await _initializationSemaphore.WaitAsync();
			if (!_initializationTaskTcs.Task.IsCompleted && MediaDataStore.Data.MediaInstances?.Any() == true)
			{
				await InitializeClientJs();
				_initializationTaskTcs.SetResult(true);
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
					return new List<Rectangle>(tags.Where(t => t.TagHyperId.Equals(position)).Select(ConvertToRectangle));
				}
				else if (TaskDataStore.Data.CurrentTask.ContentMode == ContentModes.Image || TaskDataStore.Data.CurrentTask.IsComparative())
				{
					return new List<Rectangle>(tags.Select(ConvertToRectangle));
				}
			}

			return new List<Rectangle>();
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

		private async Task InitializeClientJs()
		{
			await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.setupTaggingSurface", new object[] { _componentJsReference, _componentId });
		}

		private HyperId GetCurrentPosition()
		{
			if (MediaDataStore != null && MediaDataStore.Data.MediaInstances.Count == 1)
			{
				return MediaDataStore.Data.MediaInstances[0].CurrentPosition;
			}

			return TaskDataStore.Data.CurrentTask?.HyperId ?? new HyperId();
		}

		private async Task UpdateTagsOnClient(List<Rectangle> oldRectangleCollection, List<Rectangle> newRectangleCollection)
		{
			await Task.WhenAll(_initializationTaskTcs.Task, _currentPositionFrameRendered.WaitAsync());

			try
			{
				await _updateTagsClientSemaphore.WaitAsync();

				// update tags
				foreach (var newTag in newRectangleCollection)
				{
					var oldTag = oldRectangleCollection.SingleOrDefault(t => t.Id == newTag.Id);
					if (oldTag != null && !oldTag.Equals(newTag))
					{
						await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.updateTag", new object[] { _componentId, newTag });
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
						await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.removeTag", new object[] { _componentId, oldTag });
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
						await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.addTag", new object[] { _componentId, newTag });
					}
				}
			}
			catch { }
			finally
			{
				_updateTagsClientSemaphore.Release();
			}
		}

		private void SetSelectionMode(TagsSelectionMode mode)
		{
			ActionDispatcher.Dispatch(SetTagsSelectionModeAction.Create(mode));
		}

		protected async Task OverlayMediaWithCanvas(bool overlay)
		{
			await _initializationTaskTcs.Task;
			await this.JSRuntime.InvokeVoidAsync("Orions.Dom.setStyle", new object[] { ".tagging-canvas, .tagging-surface-child-content", new { visibility = overlay ? "visible" : "hidden" } });
		}

		protected async Task OnMediaPaused()
		{
			_mediaPaused = true;
			this.OverlayMediaWithCanvas(true);
		}

		protected override void Dispose(bool disposing)
		{
			JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.dispose", new object[] { _componentId });

			base.Dispose(disposing);
		}
	}
}
