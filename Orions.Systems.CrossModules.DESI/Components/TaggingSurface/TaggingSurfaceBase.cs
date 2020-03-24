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

		[Parameter]
		public byte[] ImageData
		{
			get
			{
				return _imageData;
			}
			set
			{
				if(value != null && value != _imageData)
				{
					_initializationRequired = true;
				}

				_imageData = value;
			}
		}

		[Parameter]
		public EventCallback<Model.Rectangle> OnTagAdded { get; set; }

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		protected string ComponentId { get { return $"tagging-surface-{this._componentId}"; } }

		private string _componentId { get; set; }
		private DotNetObjectReference<TaggingSurfaceBase> _componentJsReference { get;set; }

		public TaggingSurfaceBase()
		{
			_componentId = Guid.NewGuid().ToString();
			_componentJsReference = DotNetObjectReference.Create<TaggingSurfaceBase>(this);
		}

		[JSInvokable]
		public async Task TagAdded(Model.Rectangle rectangle)
		{
			await this.OnTagAdded.InvokeAsync(rectangle);
		}

		protected override void OnAfterRender(bool firstRender)
		{
			if (_initializationRequired)
			{
				InitializeClientJs();
				_initializationRequired = false;
			}
			base.OnAfterRender(firstRender);
		}

		private void InitializeClientJs()
		{
			JSRuntime.InvokeVoidAsync("Orions.TaggingSurface.setupTaggingSurface", new object[] { _componentJsReference, ComponentId });
		}
	}
}
