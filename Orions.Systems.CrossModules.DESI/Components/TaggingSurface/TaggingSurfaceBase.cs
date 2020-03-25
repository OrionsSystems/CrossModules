using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface
{
	public class TaggingSurfaceBase : DesiBaseComponent<TaggingViewModel>
	{
		private byte[] _imageData;
		private bool _initializationRequired;
		private List<Model.Rectangle> _rectangles = new List<Model.Rectangle>();
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
				if(value != null && (_imageData == null || !value.SequenceEqual(_imageData)))
				{
					_initializationRequired = true;
				}

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

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (_initializationRequired)
			{
				_initializationRequired = false;
				await InitializeClientJs();
			}

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
			if(_imageData != null)
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
