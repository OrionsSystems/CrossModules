using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardOption
	{
		public string Name { get; set; } = "New Dashboard";

		public bool IsDefault { get; set; }

		public bool IsHideTitle { get; set; }

		public DashboardOption()
		{
		}
	}
}
