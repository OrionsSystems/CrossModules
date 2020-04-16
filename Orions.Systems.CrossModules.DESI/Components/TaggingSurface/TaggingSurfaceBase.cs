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
		private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
		private List<Rectangle> _rectangles = new List<Rectangle>();
		private IMediaDataStore _mediaDataStore;
		private ITagsStore _tagsStore;
		private ITaskDataStore _taskDataStore;
		private bool _initializationDone;
		private SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(1, 1);
		private SemaphoreSlim _updateTagsClientSemaphore = new SemaphoreSlim(1, 1);
		private TaskCompletionSource<bool> _initializationTaskTcs = new TaskCompletionSource<bool>();
		private TaskCompletionSource<bool> _frameRenderedTaskTcs = new TaskCompletionSource<bool>();
		private DotNetObjectReference<TaggingSurfaceBase> _componentJsReference { get; set; }

		[Parameter]
		public IMediaDataStore MediaDataStore
		{
			get => _mediaDataStore;
			set => SetProperty(ref _mediaDataStore,
				value,
				() =>
				{
					_subscriptions.Add(
						value.Data.GetPropertyChangedObservable().Where(i => i.EventArgs.PropertyName == nameof(MediaData.MediaInstances)).Subscribe(_ =>
						{
							UpdateRectangles();
							if (value?.Data?.MediaInstances != null)
							{
								_subscriptions.Add(value.Data.MediaInstances[0].GetPropertyChangedObservable().Where(i => i.EventArgs.PropertyName == nameof(MediaInstance.CurrentPosition)).Subscribe(_ => UpdateRectangles()));

								_subscriptions.Add(
									value.Data.MediaInstances[0].GetPropertyChangedObservable().Where(i => i.EventArgs.PropertyName == nameof(MediaInstance.CurrentPositionFrameImage)).Subscribe(_ =>
									{
										UpdateRectangles();
									}));
							}
						}));

					_subscriptions.Add(value.Data.MediaInstances[0].GetPropertyChangedObservable().Where(i => i.EventArgs.PropertyName == nameof(MediaInstance.CurrentPosition)).Subscribe(_ => UpdateRectangles()));

					_subscriptions.Add(
						value.Data.MediaInstances[0].GetPropertyChangedObservable().Where(i => i.EventArgs.PropertyName == nameof(MediaInstance.CurrentPositionFrameImage)).Subscribe(_ =>
						{
							UpdateRectangles();
						}));
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

					_subscriptions.AddItem(aggregatedSub);

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

		private List<Rectangle> Rectangles
		{
			get => _rectangles;
			set
			{
				UpdateTagsOnClient(_rectangles, value);
				_rectangles = value;
			}
		}

		[Parameter]
		public RenderFragment ChildContent { get; set; }

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		protected string ComponentId { get { return $"tagging-surface-{this._componentId}"; } }

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
			await _frameRenderedTaskTcs.Task;
			if (this.Rectangles.Any(r => r.Id == rectangleId))
			{
				await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.attachElementPositionToTag", new object[] { rectangleId, elementSelector });
			}
		}

		[JSInvokable]
		public async Task TagPositionOrSizeChanged(Rectangle rectangle) => OnTagPositionOrSizeChanged(rectangle);

		[JSInvokable]
		public async Task FrameImageRendered()
		{
			if (!_frameRenderedTaskTcs.Task.IsCompleted)
			{
				_frameRenderedTaskTcs.SetResult(true);
			}
		}

		protected override async Task OnAfterRenderAsyncSafe(bool firstRender)
		{
			await _initializationSemaphore.WaitAsync();
			if (!_initializationDone && MediaDataStore.Data.MediaInstances?.Any() == true)
			{
				await InitializeClientJs();
				_initializationDone = true;
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

		private void OnCurrentTaskChanged()
		{
			UpdateRectangles();
			JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.zoom", new object[] { 1 });
		}

		private void UpdateRectangles() => Rectangles = CurrentPositionRectanglesSelector(TagsStore.Data.CurrentTaskTags ?? Enumerable.Empty<TagModel>());

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
			await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.setupTaggingSurface", new object[] { _componentJsReference, ComponentId });
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
			await Task.WhenAll(_initializationTaskTcs.Task, _frameRenderedTaskTcs.Task);

			try
			{
				await _updateTagsClientSemaphore.WaitAsync();

				// update tags
				foreach (var newTag in newRectangleCollection)
				{
					var oldTag = oldRectangleCollection.SingleOrDefault(t => t.Id == newTag.Id);
					if (oldTag != null && !oldTag.Equals(newTag))
					{
						await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.updateTag", new object[] { newTag });
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
						await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.removeTag", new object[] { oldTag });
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
						await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.addTag", new object[] { newTag });
					}
				}
			}
			catch { }
			finally
			{
				_updateTagsClientSemaphore.Release();
			}
		}

		protected async Task OverlayMediaWithCanvas(bool overlay)
		{
			await _initializationTaskTcs.Task;
			await this.JSRuntime.InvokeVoidAsync("Orions.Dom.setStyle", new object[] { ".tagging-canvas, .tagging-surface-child-content", new { visibility = overlay ? "visible" : "hidden" } });
		}

		protected override void Dispose(bool disposing)
		{
			JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.dispose");

			if (disposing)
			{
				_subscriptions.ForEach(i => i.Dispose());
				_subscriptions.Clear();
			}
			base.Dispose(disposing);
		}
	}
}
