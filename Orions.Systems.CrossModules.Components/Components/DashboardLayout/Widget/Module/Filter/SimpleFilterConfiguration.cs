using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class SimpleFilterConfiguration
	{
		public enum SimpleFilterOrientation
		{ 
			Inline,
			Block
		}

		[HelpText("Filter view orientation")]
		public SimpleFilterOrientation Orientation { get; set; } = SimpleFilterOrientation.Inline;

		[HelpText("Show or hide the loader")]
		public bool Visible { get; set; } = true;

		[HelpText("Separator height in px")]
		public int Height { get; set; } = 2;

		[HelpText("Valid CSS color string")]
		public string Color { get; set; } = "#365c7e";

		public SimpleFilterConfiguration()
		{
		}
	}
}
