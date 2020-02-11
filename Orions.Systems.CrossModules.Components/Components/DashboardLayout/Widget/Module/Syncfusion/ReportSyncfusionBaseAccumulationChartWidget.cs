using Orions.Common;
using Syncfusion.EJ2.Blazor;
using Syncfusion.EJ2.Blazor.Charts;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportSyncfusionBaseAccumulationChartWidget : ReportChartWidget, IDashboardWidget
	{

		public class ChartSeriesConfiguration 
		{
			[HelpText("The data label for the series.")]
			public AccumulationDataLabelSettings DataLabel { get; set; } = new AccumulationDataLabelSettings();

			[HelpText("AccumulationSeries y values less than groupTo are combined into single slice named others")]
			public string GroupTo { get; set; }

			[HelpText("Specifies the series name")]
			public string Name { get; set; }

			[HelpText("If set true, series points will be exploded on mouse click or touch.")]
			public bool Explode { get; set; }

			[HelpText("If set true, all the points in the series will get exploded on load.")]
			public bool ExplodeAll { get; set; }

			[HelpText("Options for customizing the border of the series.")]
			public AccumulationChartSeriesBorder Border { get; set; } = new AccumulationChartSeriesBorder();

			//[HelpText("Specifies the theme for accumulation chart.")]
			//public AccumulationTheme Theme { get; set; } = AccumulationTheme.Material;
		}

		public class TooltipConfiguration
		{
			[HelpText("Enables / Disables the visibility of the tooltip.")]
			public bool Enable { get; set; } = true;

			[HelpText("Format the ToolTip content.")]
			public string Format { get; set; }

			[HelpText("The fill color of the tooltip that accepts value in hex and rgba as a valid CSS color string.")]
			public double Opacity { get; set; } = 0.75;

			[HelpText("Duration for the ToolTip animation.")]
			public double Duration { get; set; } = 300;

			[HelpText(" Options to customize the ToolTip text.")]
			public AccumulationChartTooltipTextStyle TextStyle { get; set; } = new AccumulationChartTooltipTextStyle();

			[HelpText("Options to customize tooltip borders.")]
			public AccumulationChartTooltipBorder Border { get; set; } = new AccumulationChartTooltipBorder();
		}

		[HelpText("The height of the chart as a string in order to provide input as both like '100px' or '100%'. If specified as '100%, chart will render to the full height of its parent element.")]
		public string Height { get; set; }

		[HelpText("The width of the chart as a string in order to provide input as both like '100px' or '100%'. If specified as '100%, chart will render to the full width of its parent element.")]
		public string Width { get; set; }

		[HelpText("Show or hide title for accumulation chart")]
		public bool IsShowChartTitle { get; set; } = false;

		[HelpText("Title for accumulation chart")]
		public string ChartTitle { get; set; }

		[HelpText("Options for customizing the `title` of accumulation chart.")]
		public AccumulationChartTitleStyle TitleStyle { get; set; }

		[HelpText("Options for customizing the legend of accumulation chart.")]
		public SyncfiusionLegendDefinition LegendSettings { get; set; } = new SyncfiusionLegendDefinition();

		[HelpText("Enables / Disables the visibility of the tooltip.")]
		public bool IsEnableTooltip { get { return TooltipSettings.Enable; } set { TooltipSettings.Enable = value; } }

		[HelpText("Tooltip settings")]
		public TooltipConfiguration TooltipSettings { get; set; } = new TooltipConfiguration();

		[HelpText("Options to customize the left, right, top and bottom margins of accumulation chart.")]
		public AccumulationChartMargin Margin { get; set; }

		[HelpText("The configuration for series in accumulation chart.")]
		public ChartSeriesConfiguration SeriesSettings { get; set; } = new ChartSeriesConfiguration();

		[HelpText("")]
		public AccEmptyPointMode PointMode { get; set; } = AccEmptyPointMode.Zero;

		[HelpText("Specifies whether point has to get selected or not. Takes value either 'None'or 'Point'")]
		public AccumulationType AccumulationType { get; set; } = AccumulationType.Pie;

		public string Radius { get; set; } = "100%";

		public string InnerRadius { get; set; } = "0%"; 

		public ReportSyncfusionBaseAccumulationChartWidget()
		{
		}
	}
}
