using Orions.Common;
using Syncfusion.EJ2.Blazor.Charts;

namespace Orions.Systems.CrossModules.Components
{
	public class SyncfiusionLegendDefinition
	{
		[HelpText("If set to true, legend will be visible.")]
		public bool Visible { get; set; } = true;
		
		[HelpText("Position of the legend in the chart")]
		public LegendPosition Position { get; set; } = LegendPosition.Top;

		[HelpText("Legend in chart can be aligned ")]
		public Alignment Alignment { get; set; } = Alignment.Far;
		
		[HelpText("Options to customize the legend text.")]
		public AccumulationChartLegendFont TextStyle { get; set; }

		[HelpText("Shape width of the legend in pixels.")]
		public double ShapeWidth { get; set; } = 10;

		[HelpText("Padding between the legend shape and text.")]
		public double ShapePadding { get; set; } = 5;

		[HelpText("Shape height of the legend in pixels.")]
		public double ShapeHeight { get; set; } = 10;

		[HelpText("Option to customize the padding between legend items.")]
		public double Padding { get; set; } = 8;

		[HelpText("Opacity of the legend.")]
		public double Opacity { get; set; } = 1;

		[HelpText("Options to customize left, right, top and bottom margins of the chart.")]
		public MarginModel Margin { get; set; }

		[HelpText("The height of the legend in pixels.")]
		public string Height { get; set; }

		[HelpText(" The width of the legend in pixels.")]
		public string Width { get; set; }

	}
}
