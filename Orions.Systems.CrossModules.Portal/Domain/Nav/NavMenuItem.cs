using Orions.SDK;

namespace Orions.Systems.CrossModules.Portal
{
	public class NavMenuItem
	{
		public string Address { get; set; }

		public string Alias { get; set; }

		public string Label { get; set; }

		public string Description { get; set; }

		public ModuleVm Source { get; set; }

		public string MatIcon { get; set; }

		public string ImageSource { get; set; }

		public bool EnableLeftMenu { get; set; }

		public bool ForceReload { get; set; } = true;
	}
}
