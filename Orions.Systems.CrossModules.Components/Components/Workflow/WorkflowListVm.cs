using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WorkflowListVm : BlazorVm
	{

		public bool IsLoadedData { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public PropertyGridVm PropertyGridVm { get; set; } = new PropertyGridVm();

		public bool IsShowProperty { get; set; }

		public List<WorkflowConfigurationVm> Items { get; set; } = new List<WorkflowConfigurationVm>();

		public WorkflowConfigurationVm SelectedItem { get; set; }

		public string SearchInput { get; set; }

		public EventCallback<HyperWorkflow> OnManageWorkflow { get; set; }

		public EventCallback<HyperWorkflow> OnOpenWorkflowInstances { get; set; }

		public WorkflowListVm()
		{

		}

		public async Task Init()
		{
			if (HyperStore == null) return;

			await PopulateWorkflows();

			IsLoadedData = true;
		}

		public async Task ManageWorkflowAsync(WorkflowConfigurationVm item)
		{
			await OnManageWorkflow.InvokeAsync(item.Source);
		}

		public async Task OpenWorkflowInstancesAsync(WorkflowConfigurationVm item)
		{
			await OnOpenWorkflowInstances.InvokeAsync(item.Source);
		}

		private async Task PopulateWorkflows()
		{
			Items.Clear();
			IsLoadedData = false;

			var findArgs = new FindHyperDocumentsArgs();
			findArgs.SetDocumentType(typeof(HyperWorkflow));
			var workflows = await HyperStore.ExecuteAsync(findArgs);

			if (workflows == null || !workflows.Any())
				return;

			var statsArgs = new RetrieveHyperWorkflowsStatusesArgs();
			var statuses = await HyperStore.ExecuteAsync(statsArgs);

			if (statuses == null)
			{
				statuses = new HyperWorkflowStatus[] { };
			}

			foreach (var workflow in workflows)
			{
				var configuration = workflow.GetPayload<HyperWorkflow>();
				if (configuration == null)
				{
					Console.WriteLine($"Failed to load workflow from document: {workflow.Id}");
					continue;
				}

				var wfVm = new WorkflowConfigurationVm
				{
					Source = configuration,
					WorkflowStatuses = statuses.Where(it => it.Configuration.Id == configuration.Id).ToArray()
				};

				Items.Add(wfVm);
			}

			IsLoadedData = true;
		}

		public async Task OnSearchBtnClick(MouseEventArgs e)
		{
			//TODO
		}

		public async Task CreateNew()
		{
			//TODO
		}

		public void ShowPropertyGrid(WorkflowConfigurationVm item)
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
