using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Orions.Common;

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
		public EventCallback<double[][]> OnAreaAdded { get; set; }

		protected override async Task OnFirstAfterRenderAsync()
		{
			var thisReference = DotNetObjectReference.Create(this);

			if (JsInterop != null)
			{
				await JsInterop.InvokeAsync<object>("window.Orions.SvgMapEditor.init", new object[] { ComponentContainerId, thisReference, Vm.Cameras });
			}
		}

		protected override void OnDataContextAssigned(BaseVm dataContext)
		{
			

			base.OnDataContextAssigned(dataContext);
		}

		[JSInvokable]
		public async Task OnAreaAddedJS(double[][] areaVerticies)
		{
			await OnAreaAdded.InvokeAsync(areaVerticies);
		}
	}
}
