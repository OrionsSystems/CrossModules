using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class LoaderConfiguration
	{
		[HelpText("Show or hide the loader")]
		public bool Visible { get; set; } = true;

		[HelpText("Separator height in px")]
		public int Height { get; set; } = 2;

		[HelpText("Valid CSS color string")]
		public string Color { get; set; } = "#395D7F";

		[HelpText("The opacity of the marker.")]
		public double Opacity { get; set; } = 1;

		[HelpText("Add styles to title")]
		public string InlineStyles { get; set; }
	}
}
