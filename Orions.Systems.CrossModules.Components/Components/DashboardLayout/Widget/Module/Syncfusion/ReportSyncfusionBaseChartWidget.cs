using Orions.Common;
using Syncfusion.EJ2.Blazor;
using Syncfusion.EJ2.Blazor.Charts;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportSyncfusionBaseChartWidget : ActiveFilterReportChartWidget
	{
		public class TooltipConfiguration {

			[HelpText("Enables / Disables the visibility of the tooltip.")]
			public bool IsEnable { get; set; } = true;

			[HelpText("Format the ToolTip content.")]
			public string Format { get; set; }

			[HelpText("Options to customize the ToolTip text.")]
			public ChartTooltipTextStyle TextStyle { get; set; } = new ChartTooltipTextStyle();
		}

		public class ChartMargin 
		{
			public int Top { get; set; }
			public int Left { get; set; }
			public int Right { get; set; }
			public int Bottom { get; set; }
		}

		public class PrimaryXAxis
		{
			public bool Visible { get; set; } = true;

			public bool XAxisIsIndexed { get; set; } = true;

			public LabelPlacement LabelPlacement { get; set; } = LabelPlacement.OnTicks;

			public IntervalType IntervalType { get; set; } = IntervalType.Auto;

			public string LabelFormat { get; set; } = "dd MMM HH:mm";

			public ChartAxisLabelStyle LabelStyle { get; set; } = new ChartAxisLabelStyle();

			public string Skeleton { get; set; } = "Ed";

			public EdgeLabelPlacement EdgeLabelPlacement { get; set; } = EdgeLabelPlacement.Shift;

			public ChartAxisMajorGridLines MajorGridLines { get; set; } = new ChartAxisMajorGridLines();

			public ValueType ValueType { get; set; } = ValueType.Category;

			//public int Minimum { get; set; }
			//public int Maximum { get; set; }

			public int Interval { get; set; } = 1;

			public AxisPosition TickPosition { get; set; } = AxisPosition.Inside;

			public AxisPosition LabelPosition { get; set; } = AxisPosition.Outside;

			public double LabelRotation { get; set; }

			public LabelIntersectAction LabelIntersectAction { get; set; } = LabelIntersectAction.Rotate45;
		}

		public class PrimaryYAxis {
			public int Minimum { get; set; }

			public bool Visible { get; set; } = true;

			//public int Interval { get; set; } = 1;
			//public int Maximum { get; set; }

			public ChartAxisLineStyle LineStyle { get; set; } = new ChartAxisLineStyle();

			public ChartAxisMajorTickLines MajorTickLines { get; set; } = new ChartAxisMajorTickLines();

			public ChartAxisMinorTickLines MinorTickLines { get; set; } = new ChartAxisMinorTickLines();

			public ChartAxisMajorGridLines GridLineSettings { get; set; } = new ChartAxisMajorGridLines();
		}

		public class ChartMarkerConfiguration 
		{
			[HelpText("The opacity of the marker.")]
			public double Opacity { get; set; } = 1;

			[HelpText("The height of the marker in pixels.")]
			public double Height { get; set; }

			[HelpText("The fill color of the marker that accepts value in hex and rgba as a valid CSS color string. By default, it will take series' color.")]
			public string Fill { get; set; }

			[HelpText("Options for customizing the border of a marker.")]
			public ChartMarkerBorder Border { get; set; }

			[HelpText("The width of the marker in pixels.")]
			public double Width { get; set; }

			[HelpText("If set to true the marker for series is rendered. This is applicable only for line and area type series.")]
			public bool Visible { get; set; }

			[HelpText("Specifies the position of the data label.")]
			public LabelPosition Position { get; set; } = LabelPosition.Auto;

			[HelpText("Option for customizing the data label text.")]
			public ChartDataLabelFont Font { get; set; }
		}

		[HelpText("Options for chart marker")]
		public ChartMarkerConfiguration ChartMarkerSettings { get; set; } = new ChartMarkerConfiguration();

		[HelpText("The height of the chart as a string accepts input both as '100px' or '100%'. If specified as '100%, chart renders to the full height of its parent element.")]
		public string Height { get; set; }

		[HelpText("The width of the chart as a string accepts input as both like '100px' or '100%'. If specified as '100%, chart renders to the full width of its parent element.")]
		public string Width { get; set; }

		public bool IsShowChartTitle { get; set; } = false;

		[HelpText("Title of the chart")]
		public string ChartTitle { get; set; }

		[HelpText("To render the column series points with particular column spacing. It takes value from 0 - 1.")]
		public double ColumnSpacing { get; set; } = 0;

		public SelectionMode SelectionMode { get; set; } = SelectionMode.Series;

		public bool IsEnableTooltip { get { return TooltipSettings.IsEnable; } set { TooltipSettings.IsEnable = value; } }

		public TooltipConfiguration TooltipSettings { get; set; } = new TooltipConfiguration();

		public SyncfiusionLegendDefinition LegendSettings { get; set; } = new SyncfiusionLegendDefinition();

		public ChartSeriesType ChartSeriesType { get; set; } = ChartSeriesType.StackingColumn;

		public bool IsShowMarker { get { return ChartMarkerSettings.Visible; } set { ChartMarkerSettings.Visible = value; } }

		public string ChartBackground { get; set; } = "transparent";

		public bool EnableMouseWheelZooming { get; set; }

		public bool EnablePinchZooming { get; set; }

		public bool EnableSelectionZooming { get; set; }

		public bool EnablePan { get; set; }

		public bool EnableScrollbarZooming { get; set; }

		public bool IsEnableCrosshair { get; set; }

		public PrimaryXAxis XAxisSettings { get; set; } = new PrimaryXAxis();

		public PrimaryYAxis YAxisSettings { get; set; } = new PrimaryYAxis();

		public ChartCornerRadius CornerRadius { get; set; } = new ChartCornerRadius();

		public ChartMargin Margin { get; set; } = new ChartMargin();

		public ChartAreaBorder Border { get; set; } = new ChartAreaBorder() { Width = 0 };

		public ReportSyncfusionBaseChartWidget()
		{
		}
	}
}
