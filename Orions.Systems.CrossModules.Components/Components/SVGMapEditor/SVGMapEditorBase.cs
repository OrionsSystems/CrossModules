using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Orions.Common;
using System.Collections.Generic;
using Orions.Infrastructure.HyperMedia.MapOverlay;
using Orions.Node.Common;
using Orions.Infrastructure.HyperMedia;
using Microsoft.AspNetCore.Components.Web;
using System;

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor
{
	public class SVGMapEditorBase : BaseBlazorComponent<SVGMapEditorVm>, IDisposable
	{
		protected string ComponentContainerId;
		protected override bool AutoCreateVm { get; } = false;

		public SVGMapEditorBase()
		{
			ComponentContainerId = $"{base.Id}{nameof(SVGMapEditor)}-component-container";
		}

		[Parameter]
		public RenderFragment ChildContent { get; set; }

		[Parameter]
		public IHyperArgsSink HyperStore { get; set; }

		protected override void OnInitialized()
		{
			base.OnInitialized();
		}

		protected override async Task OnFirstAfterRenderAsync()
		{
			this.Vm.JsRuntime = JsInterop;
			var thisReference = DotNetObjectReference.Create(this);
			await this.Vm.Initialize(ComponentContainerId, thisReference);
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (Vm.IsMapOverlayInitialized && Vm.MapInitialized == false)
			{
				Vm.MapInitialized = true;
				await this.Vm.InitializeMapJs();
			}

			await base.OnAfterRenderAsync(firstRender);
		}

		[JSInvokable]
		public async Task<JsModel.ZoneOverlayEntryJsModel> AddNewZoneToVm(JsModel.ZoneOverlayEntryJsModel zone)
		{
			zone = Vm.AddNewZoneToVm(zone);

			return zone;
		}

		[JSInvokable]
		public async Task OpenSvgControlProps(JsModel.ZoneOverlayEntryJsModel zone, SvgComponentEvent e)
		{
			this.Vm.OpenSvgControlProps(zone.Id);
		}

		[JSInvokable]
		public async Task ShowTagInfo(JsModel.CircleOverlayEntryJsModel circle, SvgComponentEvent e)
		{
			await Vm.ShowTagInfo(circle, e.ClientX.Value, e.ClientY.Value);
		}

		[JSInvokable]
		public async Task RemoveTagCirclesForZone(JsModel.ZoneOverlayEntryJsModel zone, SvgComponentEvent e)
		{
			await Vm.RemoveTagCirclesForZone(zone.Id);
		}

		[JSInvokable]
		public async Task UpdateZone(JsModel.ZoneOverlayEntryJsModel zone, SvgComponentEvent e)
		{
			await Vm.UpdateZone(zone);
		}

		[JSInvokable]
		public async Task CloseHyperTagInfoPopup()
		{
			if(this.Vm.ShowingHyperTagProperties.Value != true)
			{
				this.Vm.ShowingHyperTagInfo.Value = false;
			}
		}

		[JSInvokable]
		public async Task DeleteZone(JsModel.ZoneOverlayEntryJsModel zone, SvgComponentEvent e)
		{
			await this.Vm.DeleteZone(zone);
		}

		[JSInvokable]
		public async Task SelectZone(JsModel.ZoneOverlayEntryJsModel zone, SvgComponentEvent e)
		{
			Vm.SelectZone(zone);

			this.StateHasChanged();
		}

		[JSInvokable]
		public async Task UnselectZone(JsModel.ZoneOverlayEntryJsModel zone, SvgComponentEvent e)
		{
			Vm.UnselectZone(zone);
		}

		public void Dispose()
		{
			Vm.Dispose();
		}
	}

	public class SvgComponentEvent
	{
		public double? PageX { get; set; }
		public double? PageY { get; set; }
		public double? ClientX { get; set; }
		public double? ClientY { get; set; }
	}

}
