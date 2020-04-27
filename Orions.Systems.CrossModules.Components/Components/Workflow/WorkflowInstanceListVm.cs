using Microsoft.AspNetCore.Components;

using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WorkflowInstanceListVm : BlazorVm
	{

		public bool IsLoadedData { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public PropertyGridVm PropertyGridVm { get; set; } = new PropertyGridVm();

		public bool IsShowProperty { get; set; }

		public List<WorkflowInstanceVm> Items { get; set; } = new List<WorkflowInstanceVm>();

		public WorkflowInstanceVm SelectedItem { get; set; }

		//public HyperWorkflowStatus WorkflowStatus { get; set; }

		public string WorkflowId { get; set; }

		public EventCallback<HyperWorkflowStatus> OnManageWorkflowInstance { get; set; }

		public EventCallback<HyperWorkflowStatus> OnOpenWorkflowHistory { get; set; }

		public WorkflowInstanceListVm()
		{
		}

		public async Task Init()
		{
			if (HyperStore == null) return;

			await PopulateData();

			IsLoadedData = true;
		}

		public async Task ManageWorkflowAsync(WorkflowInstanceVm item)
		{
			await OnManageWorkflowInstance.InvokeAsync(item.Source);
		}

		public async Task OpenWorkflowHistoryAsync(WorkflowInstanceVm item) 
		{
			await OnOpenWorkflowHistory.InvokeAsync(item.Source);
		}

		private async Task PopulateData()
		{
			Items.Clear();
			IsLoadedData = false;

			if (HyperStore == null || string.IsNullOrWhiteSpace(WorkflowId))
				return;

			//var documentId = HyperDocumentId.Create<HyperWorkflow>(WorkflowId);
			//var wfArgs = new RetrieveHyperDocumentArgs(documentId);
			//var wfDoc = await HyperStore.ExecuteAsync(wfArgs);

			//if (wfArgs.ExecutionResult.IsNotSuccess)
			//	return;

			//var workflow = wfDoc?.GetPayload<HyperWorkflow>();


			var statsArgs = new RetrieveHyperWorkflowsStatusesArgs { WorkflowConfigurationIds = new string[] { WorkflowId } };
			//var statsArgs = new RetrieveHyperWorkflowsStatusesArgs();
			var statuses = await HyperStore.ExecuteAsync(statsArgs);

			if (statuses != null && statuses.Any())
			{
				var workflowStatuses = statuses.OrderBy(it => it.PrintTitle).ToList();
				foreach (var st in workflowStatuses)
				{
					var wfInstVm = new WorkflowInstanceVm() { Source = st, HyperStore = HyperStore };
					Items.Add(wfInstVm);
				}
			}

			IsLoadedData = true;
		}

		public void ShowPropertyGrid(WorkflowInstanceVm item)
		{
			SelectedItem = item;
			IsShowProperty = true;
		}

		public Task<object> LoadPropertyGrid()
		{
			return Task.FromResult<object>(SelectedItem.Source);
		}

		public void OnCancelProperty()
		{
			PropertyGridVm.CleanSourceCache();
			IsShowProperty = false;
		}
	}
}
