using System.Collections.Generic;
using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardContainerWidget : DashboardWidget, IDashboardWidget
	{
		public DashboardLayout DashboardLayout;

		public string DashboardElementName { get; set; }

		public DashboardContainerWidget()
		{
			this.Label = "Dashboard container";
		}
	}
}
