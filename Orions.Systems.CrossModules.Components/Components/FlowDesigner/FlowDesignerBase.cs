using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Orions.Node.Common;

using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class FlowDesignerBase : BaseBlazorComponent<FlowDesignerVm>, IDisposable
	{
		IDisposable thisReference;

		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => Vm.HyperStore;
			set => Vm.HyperStore = value;
		}

		PropertyGrid _propGrid;
		PropertyGrid propGrid
		{
			get => _propGrid;
			set
			{
				_propGrid = value;
				Vm.PropertyGridVm = value.Vm;
			}
		}

		public FlowDesignerBase()
		{

		}

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

		}

		protected override async Task OnFirstAfterRenderAsync()
		{
			thisReference = DotNetObjectReference.Create(this);
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.Init", new object[] { thisReference });

		}

		void IDisposable.Dispose()
		{
			thisReference?.Dispose();
		}
	}
}
