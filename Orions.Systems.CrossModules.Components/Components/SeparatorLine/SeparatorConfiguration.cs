using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class SeparatorConfiguration
	{
		public enum SeparatorAligment
		{
			Left,
			Center,
			Right
		}

		[HelpText("Assign margin values to an element")]
		public MarginConfig Margin { get; set; } = new MarginConfig();

		[HelpText("Assign padding values to an element")]
		public PaddingConfig Padding { get; set; } = new PaddingConfig();

		[HelpText("CSS class name")]
		public string CssClass { get; set; }

		[HelpText("Show or hide the title")]
		public bool Visible { get; set; } = false;

		[HelpText("Separator width in percentage")]
		public int WidthPercentage { get; set; } = 100;

		[HelpText("Separator height in px")]
		public int Height { get; set; } = 2;

		[HelpText("Valid CSS color string")]
		public string Color { get; set; } = "#365c7e";

		[HelpText("The opacity of the marker.")]
		public double Opacity { get; set; } = 1;

		[HelpText("The opacity of the separator")]
		public SeparatorAligment Aligment { get; set; } = SeparatorAligment.Center;

		[HelpText("Add styles to title")]
		public string InlineStyles { get; set; }

		public string GetPercentageWidth() 
		{
			return $"{WidthPercentage}%";
		}

		public string GetHeightInPx()
		{
			return $"{Height}px";
		}
	}
}
