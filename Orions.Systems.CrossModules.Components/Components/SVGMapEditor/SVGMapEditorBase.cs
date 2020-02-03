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

		protected override async Task OnFirstAfterRenderAsync()
		{
			await this.Vm.Initialize();

			this.Vm.MapOverlay.Value.PopulateTypeSpecificCollections();

			var thisReference = DotNetObjectReference.Create(this);
			await JsInterop.InvokeAsync<object>("window.Orions.SvgMapEditor.init", new object[] { ComponentContainerId, thisReference, this.Vm.MapOverlay.Value });
		}

		[JSInvokable]
		public async Task SaveMapOverlay(MapOverlay overlay)
		{
			overlay.MergeEntriesIntoSingleCollection();
			await this.Vm.SaveMapOverlay(overlay);
		}
	}

}
