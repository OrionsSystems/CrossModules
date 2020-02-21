using Orions.Common;

using System;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardOption
	{
		public string Name { get; set; } = "New Dashboard";

		public string Group { get; set; }

		[HelpText("Enable client access to dashboard")]
		public bool Published { get; set; }

		public bool IsHideTitle { get; set; }

		public string Tag { get; set; }

		[HelpText("Apply css styles to the bottom of the page")]
		[UniBrowsable(UniBrowsableAttribute.EditTypes.MultiLineText)]
		public string Styles { get; set; }

		public bool EnableStyles { get; set; } = true;

		public DashboardOption()
		{
		}
	}
}
