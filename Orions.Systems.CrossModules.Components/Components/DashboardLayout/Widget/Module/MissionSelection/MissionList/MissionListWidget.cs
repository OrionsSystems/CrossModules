using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public class MissionListWidget : DashboardWidget, IDashboardWidget
	{
		public MissionListWidget()
		{
			this.Label = "Mission List";
		}
	}
}
