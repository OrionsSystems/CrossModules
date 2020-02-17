﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Orions.Common;
using System.Collections.Generic;
using Orions.Infrastructure.HyperMedia.MapOverlay;
using Orions.Node.Common;
using Orions.Infrastructure.HyperMedia;
using Microsoft.AspNetCore.Components.Web;

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor
{
	public class SVGMapEditorBase : BaseBlazorComponent<SVGMapEditorVm>
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

			//Task.Run(async () =>
			//{
			//	await Vm.TestLiveUpdate();
			//});
		}

		[JSInvokable]
		public async Task SaveMapOverlay(JsModel.MapOverlayJsModel overlay)
		{
			var mapOverlay = overlay.ToDomainModel();
			await this.Vm.SaveMapOverlay(mapOverlay);
		}

		[JSInvokable]
		public async Task<JsModel.ZoneOverlayEntryJsModel> AddNewZoneToVm(JsModel.ZoneOverlayEntryJsModel zone)
		{
			zone = Vm.AddNewZoneToVm(zone);

			return zone;
		}

		[JSInvokable]
		public async Task OpenSvgControlProps(string id)
		{
			this.Vm.OpenSvgControlProps(id);
		}

		[JSInvokable]
		public async Task ShowTagInfo(JsModel.CircleOverlayEntryJsModel circle, SvgComponentEvent e)
		{
			await Vm.ShowTagInfo(circle, e.ClientX.Value, e.ClientY.Value);
		}

		[JSInvokable]
		public async Task OpenHeatmap(string zoneId)
		{
			await Vm.OpenHeatmap(zoneId);
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
