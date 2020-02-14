namespace Orions.Systems.CrossModules.Components
{
	public class DashboardOption
	{
		public string Name { get; set; } = "New Dashboard";

		public string Group { get; set; }

		public bool IsDefault { get; set; }

		public bool IsHideTitle { get; set; }

		public string Tag { get; set; }

		public DashboardOption()
		{
		}
	}
}
