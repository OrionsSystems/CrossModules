using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Collections;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Media;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TagsExploitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rectangle = Orions.Systems.CrossModules.Desi.Components.TaggingSurface.Model.Rectangle;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface
{
	public class TaggingSurfaceBase : BaseComponent
	{
		private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

		private List<Rectangle> _rectangles = new List<Rectangle>();
		private SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(1, 1);
		private IMediaDataStore _mediaDataStore;
		private ITagsStore _tagsStore;
		private IDisposable _tagsCollectionChagedSub;
		private bool _initializationDone;
		private DotNetObjectReference<TaggingSurfaceBase> _componentJsReference { get; set; }

		[Parameter]
		public IMediaDataStore MediaDataStore
		{
			get => _mediaDataStore;
			set => SetProperty(ref _mediaDataStore,
				value,
				() => _subscriptions.Add(value.Data.GetPropertyChangedObservable().Where(i => i.EventArgs.PropertyName == nameof(MediaData.MediaInstances)).Subscribe(_ => UpdateState())));
		}

		[Parameter]
		public ITagsStore TagsStore
		{
			get => _tagsStore;
			set => SetProperty(ref _tagsStore,
				value,
				() =>
				{
					_subscriptions.AddItem(value.SelectedTagsUpdated.Subscribe(OnSelectedTagsUpdated))
					.AddItem(value.Data
					.GetPropertyChangedObservable()
					.Where(i => i.EventArgs.PropertyName == nameof(TagsExploitationData.CurrentTaskTags))
					.Subscribe(i => OnCurrentTaskTagsChanged(i.Source.CurrentTaskTags)));

					Rectangles = value.Data.CurrentTaskTags.Select(ConvertToRectangle).ToList();
				});
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

		private string _componentId { get; set; }

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
			if (_initializationDone)
			{
				if(this.Rectangles.Any(r => r.Id == rectangleId))
				{
					await JSRuntime.InvokeAsync<Model.ClientPosition>("Orions.TaggingSurface.attachElementPositionToTag", new object[] { rectangleId, elementSelector });
				}
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await _initializationSemaphore.WaitAsync();
			if (!_initializationDone && MediaDataStore.Data.MediaInstances?.Any() == true)
			{
				await InitializeClientJs();
				_initializationDone = true;
			}
			_initializationSemaphore.Release();

			await base.OnAfterRenderAsync(firstRender);
		}

		private void OnCurrentTaskTagsChanged(ReadOnlyObservableCollectionEx<TagModel> tags)
		{
			Rectangles = tags.Select(ConvertToRectangle).ToList();

			_tagsCollectionChagedSub?.Dispose();
			_tagsCollectionChagedSub = tags?
				.GetCollectionChangedObservable()
				.Select(i => i.Source.Where(t => t.TagHyperId.Equals(GetCurrentPosition())))
				.Subscribe(i => Rectangles = i.Select(ConvertToRectangle).ToList());
		}

		private Rectangle ConvertToRectangle(TagModel tagModel) => new Rectangle
		{
			X = tagModel.Geometry.ProportionalBounds.X,
			Y = tagModel.Geometry.ProportionalBounds.Y,
			Height = tagModel.Geometry.ProportionalBounds.Height,
			Width = tagModel.Geometry.ProportionalBounds.Width,
			Id = tagModel.Id.ToString(),
			IsSelected = tagModel.IsSelected
		};

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
			foreach(var tag in _rectangles)
			{
				await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.addTag", new object[] { tag });
			}
		}

		private void OnSelectedTagsUpdated(IReadOnlyCollection<TagModel> obj)
		{
			/// TODO : update selected tags on client's UI
		}

		private HyperId GetCurrentPosition()
		{
			if(MediaDataStore.Data.MediaInstances.Count == 1)
			{
				return MediaDataStore.Data.MediaInstances[0].CurrentPosition;
			}

			return MediaDataStore.Data.CurrentTaskHyperId;
		}

		private void UpdateTagsOnClient(List<Rectangle> oldRectangleCollection, List<Rectangle> newRectangleCollection)
		{
			if(_initializationDone && MediaDataStore.Data.MediaInstances?.Any() == true)
			{
				// add newly added tags
				foreach(var newTag in newRectangleCollection)
				{
					if (oldRectangleCollection.Any(t => t.Id == newTag.Id))
					{
						continue;
					}
					else
					{
						JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.addTag", new object[] { newTag });
					}
				}

				// update tags
				foreach(var newTag in newRectangleCollection)
				{
					var oldTag = oldRectangleCollection.SingleOrDefault(t => t.Id == newTag.Id);
					if (oldTag  != null && !oldTag.Equals(newTag))
					{
						JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.updateTag", new object[] { newTag });
					}
				}

				// remove removed tags
				foreach (var oldTag in oldRectangleCollection)
				{
					if(newRectangleCollection.Any(t => t.Id == oldTag.Id))
					{
						continue;
					}
					else
					{
						JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.removeTag", new object[] { oldTag });
					}
				}
			}
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
