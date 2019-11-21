using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Sandbox.Model
{
	public class WorkflowViewModel
	{
		public string Name { get; set; }
		public string WorkflowId { get; set; }
		public HyperDocument Document { get; set; }
		public HyperWorkflow Source { get; set; }
		public HyperWorkflowStatus[] WorkflowStatuses { get; set; }

		public bool HasInstances
		{
			get
			{
				if (WorkflowStatuses != null && this.WorkflowStatuses.Length > 0)
				{
					return true;
				}

				return false;
			}
		}
	}
}
