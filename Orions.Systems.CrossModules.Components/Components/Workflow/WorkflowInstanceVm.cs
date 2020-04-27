using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WorkflowInstanceVm
	{
		public IHyperArgsSink HyperStore { get; set; }

		public HyperWorkflowStatus Source { get; set; }

		public string WorkflowInstanceId
		{
			get
			{
				var source = this.Source;
				if (source != null)
					return source.WorkflowInstanceId;

				return string.Empty;
			}
		}


		public string StatusBackground 
		{ 
			get {
				if (Source == null || this.Source.WorkflowOperationalState != UniOperationalStates.Operational)
				{
					return "#808080"; //Gray
				}
				else
				{
					return "#008000"; //Green
				}
			} 
		}

		protected async Task DoRefreshAsync()
		{
			if (HyperStore == null || this.Source == null || string.IsNullOrEmpty(this.Source.WorkflowInstanceId))
				return;

			this.Source.NodeStatuses = new HyperWorkflowNodeStatus[] { }; // Clear all the statuses.

			var args = new RetrieveHyperWorkflowsStatusesArgs() { WorkflowInstancesIds = new string[] { this.Source.WorkflowInstanceId } };
			HyperWorkflowStatus[] statuses = await HyperStore.ExecuteAsync(args);
			if (statuses != null && statuses.Length > 0)
			{
				Source  = statuses.FirstOrDefault(it => it.WorkflowInstanceId == WorkflowInstanceId);
			}
		}

		public override string ToString()
		{
			return this.Source != null ? this.Source.ToString() : "[NA]";
		}
	}
}
