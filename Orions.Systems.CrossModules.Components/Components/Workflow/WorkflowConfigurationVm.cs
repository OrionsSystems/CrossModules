using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System.Linq;
using System.Threading.Tasks;
namespace Orions.Systems.CrossModules.Components
{
	public class WorkflowConfigurationVm
	{
		public IHyperArgsSink HyperStore { get; set; }

		public HyperWorkflow Source { get; set; }

		public HyperWorkflowStatus[] WorkflowStatuses { get; set; }

		public string StatusBackground
		{
			get
			{
				if (this.Source != null && this.WorkflowStatuses != null && this.WorkflowStatuses.Length > 0)
				{
					return "#008000"; //Green
				}
				else
				{
					return "#808080"; //Gray
				}
			}
		}

		public override string ToString()
		{
			return this.Source != null ? this.Source.ToString() : "[NA]";
		}
	}
}
