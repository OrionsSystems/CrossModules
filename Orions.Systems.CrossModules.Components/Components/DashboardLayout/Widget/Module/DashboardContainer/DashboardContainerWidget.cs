using Orions.Common;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardContainerWidget : DashboardWidget, IDashboardWidget
	{
		public DashboardLayout DashboardLayout;

		[HelpText("The Id of the dashboard to use", HelpTextAttribute.Priorities.Mandatory)]
		[HyperDocumentId.DocumentType(typeof(DashboardData))]
		public HyperDocumentId? Dashboard { get; set; }

		public DashboardContainerWidget()
		{
			this.Label = "Dashboard container";
		}
	}
}
