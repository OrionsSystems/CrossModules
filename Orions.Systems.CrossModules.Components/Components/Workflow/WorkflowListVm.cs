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

		public List<HyperWorkflow> Items { get; set; } = new List<HyperWorkflow>();

		public HyperWorkflow SelectedItem { get; set; }

		public string SearchInput { get; set; }

		public EventCallback<HyperWorkflow> OnManageWorkflow { get; set; }

		public WorkflowListVm()
		{
		}

		public async Task Init()
		{
			if (HyperStore == null) return;

			await PopulateWorkflows();

			IsLoadedData = true;
		}

		public async Task ManageWorkflowAsync(HyperWorkflow item)
		{
			await OnManageWorkflow.InvokeAsync(item);
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

			//var statsArgs = new RetrieveHyperWorkflowsStatusesArgs();
			//var statuses = await HyperStore.ExecuteAsync(statsArgs);

			//if (statuses == null)
			//{
			//	statuses = new HyperWorkflowStatus[] { };
			//}

			foreach (var workflow in workflows)
			{
				var configuration = workflow.GetPayload<HyperWorkflow>();
				if (configuration == null)
				{
					Console.WriteLine($"Failed to load workflow from document: {workflow.Id}");
					continue;
				}

				Items.Add(configuration);

				//var workflowStatuses = statuses.Where(it => it.Configuration.Id == configuration.Id).ToArray()

			}

			IsLoadedData = true;
		}

		public async Task OnSearchBtnClick(MouseEventArgs e)
		{
			//TODO
		}

		public async Task CreateNewWorkflow()
		{
			//TODO
		}

		public void ShowPropertyGrid(HyperWorkflow item)
		{
			SelectedItem = item;
			IsShowProperty = true;
		}

		public Task<object> LoadPropertyGrid()
		{
			return Task.FromResult<object>(SelectedItem);
		}

		public void OnCancelProperty()
		{
			PropertyGridVm.CleanSourceCache();
			IsShowProperty = false;
		}


	}
}
