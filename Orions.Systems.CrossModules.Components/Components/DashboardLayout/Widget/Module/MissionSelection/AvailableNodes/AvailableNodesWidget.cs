using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public class AvailableNodesWidget : DashboardWidget, IDashboardWidget
	{
		public AvailableNodesWidget()
		{
			this.Label = "Available Nodes";
		}
	}
}
