using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Orions.Node.Common;

using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WorkflowDesignerBase : FlowDesignerBase<WorkflowDesignerVm>
	{
		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => Vm.HyperStore;
			set => Vm.HyperStore = value;
		}

		[Parameter]
		public string WorkflowId { get => Vm.WorkflowId; set => Vm.WorkflowId = value; }

		[Parameter]
		public string WorkflowInstanceId { get => Vm.WorkflowInstanceId; set => Vm.WorkflowInstanceId = value; }

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

		public WorkflowDesignerBase()
		{

		}

		[JSInvokable]
		public async Task<string> LoadStatuses()
		{
			return await Vm.LoadWorkflowStatusesJson();
		}
	}
}
