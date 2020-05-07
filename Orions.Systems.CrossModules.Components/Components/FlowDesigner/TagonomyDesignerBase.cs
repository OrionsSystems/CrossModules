using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Orions.Node.Common;

using System;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.Components.Model;

namespace Orions.Systems.CrossModules.Components
{
	public class TagonomyDesignerBase : FlowDesignerBase<TagonomyDesignerVm>
	{
		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => Vm.HyperStore;
			set => Vm.HyperStore = value;
		}

		[Parameter]
		public string TagonomyId { get => Vm.TagonomyId; set => Vm.TagonomyId = value; }

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

		public TagonomyDesignerBase()
		{

		}

		protected override async Task OnFirstAfterRenderAsync()
		{
			await Vm.Init();

			var jsonData = Vm.GetJesonDesignData();

			thisReference = DotNetObjectReference.Create(this);

			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.Init", new object[] { thisReference, jsonData });

		}	

	}
}
