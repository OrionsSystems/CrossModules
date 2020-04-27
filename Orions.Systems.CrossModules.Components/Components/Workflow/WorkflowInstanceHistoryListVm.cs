using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WorkflowInstanceHistoryListVm : BlazorVm
	{

		public bool IsLoadedData { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public string WorkflowId { get; set; }

		public List<HyperWorkflowInstance> Items { get; set; } = new List<HyperWorkflowInstance>();


		public HyperWorkflowStatus WorkflowStatus { get; set; }

		//public HyperWorkflow Workflow { get; set; }


		public WorkflowInstanceHistoryListVm()
		{
		}

		public async Task Init()
		{
			if (HyperStore != null || string.IsNullOrWhiteSpace(WorkflowId) == false) 
			{
				await PopulateData();
			}

			IsLoadedData = true;
		}

		public string GetInstanceLabel(HyperWorkflowInstance instance) 
		{
			return string.Format("{0}, Created {1}, Id {2}", instance.Configuration?.Name, instance.CreatedAtUTC, instance.Id);
		}

		private async Task PopulateData()
		{
			Items.Clear();
			IsLoadedData = false;

			if (HyperStore == null || string.IsNullOrWhiteSpace(WorkflowId))
				return;

			var historyArgs = new FindHyperDocumentsArgs(true);
			historyArgs.DescriptorConditions.AddCondition(Assist.GetPropertyName((HyperWorkflowInstance i) => i.WorkflowId), WorkflowId);
			var instances = await FindHyperDocumentsArgs.FindAsync<HyperWorkflowInstance>(HyperStore, historyArgs);

			if (instances != null)
			{
				var wfInstances = instances.OrderBy(it => it.ToString());
				Items.AddRange(wfInstances);
			}

			IsLoadedData = true;
		}
	}
}
