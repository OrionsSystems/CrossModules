using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface
{
	public class TaggingSurfaceBase : DesiBaseComponent<TaggingViewModel>
	{
		private byte[] _imageData;
		private List<Model.Rectangle> _rectangles = new List<Model.Rectangle>();
		private bool _initializationDone;
		private string _componentId { get; set; }
		private DotNetObjectReference<TaggingSurfaceBase> _componentJsReference { get; set; }

		[Parameter]
		public byte[] ImageData
		{
			get
			{
				return _imageData;
			}
			set
			{
				_imageData = value;
			}
		}

		[Parameter]
		public List<Model.Rectangle> Rectangles
		{
			get
			{
				return _rectangles;
			}
			set
			{
				UpdateTagsOnClient(_rectangles, value);
				_rectangles = value;
			}
		}

		[Parameter]
		public EventCallback<Model.Rectangle> OnTagAdded { get; set; }

		[Parameter]
		public EventCallback<string> OnTagSelected { get; set; }

		[Parameter]
		public EventCallback<Model.Rectangle> OnTagPositionOrSizeChanged { get; set; }

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
		public async Task<string> TagAdded(Model.Rectangle rectangle)
		{
			rectangle.Id = Guid.NewGuid().ToString();

			await this.OnTagAdded.InvokeAsync(rectangle);

			return rectangle.Id;
		}

		[JSInvokable]
		public async Task TagSelected(string id)
		{
			await this.OnTagSelected.InvokeAsync(id);
		}

		public async Task AttachElementPositionToRectangle(string rectangleId, string elementSelector)
		{
			if (_initializationDone)
			{
				if(this.Rectangles.Any(r => r.Id == rectangleId))
				{
					await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.attachElementPositionToTag", new object[] { rectangleId, elementSelector });
				}

			}
		}

		[JSInvokable]
		public async Task TagPositionOrSizeChanged(Model.Rectangle rectangle)
		{
			await this.OnTagPositionOrSizeChanged.InvokeAsync(rectangle);
		}

		private SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(1, 1);
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await _initializationSemaphore.WaitAsync();
			if (!_initializationDone && _imageData != null)
			{
				await InitializeClientJs();
				_initializationDone = true;
			}
			_initializationSemaphore.Release();

			await base.OnAfterRenderAsync(firstRender);
		}

		private async Task InitializeClientJs()
		{
			await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.setupTaggingSurface", new object[] { _componentJsReference, ComponentId });
			foreach(var tag in _rectangles)
			{
				await JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.addTag", new object[] { tag });
			}
		}

		private void UpdateTagsOnClient(List<Model.Rectangle> oldRectangleCollection, List<Model.Rectangle> newRectangleCollection)
		{
			if(_imageData != null && _initializationDone)
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
					Model.Rectangle oldTag = oldRectangleCollection.SingleOrDefault(t => t.Id == newTag.Id);
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
	}
}
