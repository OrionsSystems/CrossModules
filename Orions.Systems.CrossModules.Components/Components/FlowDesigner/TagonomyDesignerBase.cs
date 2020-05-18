using Microsoft.AspNetCore.Components;

using Orions.Node.Common;

using System.Threading.Tasks;

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
			await base.OnFirstAfterRenderAsync();

			await HideMainMenu();

		}
	}
}
