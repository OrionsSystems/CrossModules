using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Sandbox.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Sandbox.Data
{
	public partial class DataContext
	{
		public async Task<WorkflowViewModel> GetWorkflowAsync(string workflowId)
		{
			if (string.IsNullOrEmpty(workflowId)) throw new ArgumentException(nameof(workflowId));

			// Find all data sources stored on this node (and visible by the current user).
			var findArgs = new FindHyperDocumentsArgs();
			findArgs.SetDocumentType(typeof(HyperWorkflow));
			var workflows = await _netStore.ExecuteAsync(findArgs);

			// Pull all working statuses.
			var statsArgs = new RetrieveHyperWorkflowsStatusesArgs();
			var workflowStatuses = await _netStore.ExecuteAsync(statsArgs);

			var workflowsData = new List<WorkflowViewModel>();

			var model = new WorkflowViewModel();

			foreach (var document in workflows)
			{
				var configuration = document.GetPayload<HyperWorkflow>();

				if (configuration == null) continue;

				if (workflowId != configuration.Id) continue;

				model.WorkflowId = configuration.Id;
				model.Name = configuration.Name;
				model.Document = document;
				model.Source = configuration;
				model.WorkflowStatuses = workflowStatuses.Where(it => it.Configuration.Id == configuration.Id).ToArray();

				return model;
			}

			throw new ApplicationException("Missing workflow");
		}


		public async Task<IEnumerable<WorkflowViewModel>> GetWorkflowsAsync(
			int pageNumber = 0,
			int pageSize = 100)
		{
			// Find all data sources stored on this node (and visible by the current user).
			var findArgs = new FindHyperDocumentsArgs();
			findArgs.SetDocumentType(typeof(HyperWorkflow));


			var docs = await _netStore.ExecuteAsync(findArgs);

			if (!docs.Any()) new List<WorkflowViewModel>();

			var workflowsIds = new List<string>();
			foreach (var document in docs)
			{
				var configuration = document.GetPayload<HyperWorkflow>();
				if (configuration == null) continue;
				workflowsIds.Add(configuration.Id);
			}

			HyperWorkflowStatus[]  statuses = await _netStore.ExecuteAsync(new RetrieveHyperWorkflowsStatusesArgs()
			{
				WorkflowConfigurationIds = workflowsIds.ToArray()
			});


			var workflowsData = new List<WorkflowViewModel>();

			foreach (var document in docs)
			{
				var configuration = document.GetPayload<HyperWorkflow>();

				if (configuration == null) continue;

				var model = new WorkflowViewModel()
				{
					WorkflowId = configuration.Id,
					Name = configuration.Name,
					Document = document,
					Source = configuration,
				};


				if (statuses != null)
				{
					model.WorkflowStatuses = statuses.Where(it => it.Configuration.Id == configuration.Id).ToArray();
				}
				else
				{
					model.WorkflowStatuses = new HyperWorkflowStatus[] { };
				}

				workflowsData.Add(model);
			}

			return workflowsData.OrderByDescending(it => it.HasInstances).ToList();

		}
	}
}
