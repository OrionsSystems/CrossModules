using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Orions.Common;
using System.Collections.Generic;
using Orions.Infrastructure.HyperMedia.MapOverlay;
using Orions.Node.Common;
using Orions.Infrastructure.HyperMedia;

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
		public async Task OpenSvgControlProps(string id)
		{
			this.Vm.OpenSvgControlProps(id);
		}
	}

}
