using Syncfusion.EJ2.Blazor;
using Syncfusion.EJ2.Blazor.Charts;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportSyncfusionBaseChartWidget : ReportChartWidget, IDashboardWidget
	{

		public class TooltipConfiguration {
			public bool IsEnable { get; set; }

			public string Format { get; set; }

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

			public LabelIntersectAction LabelIntersectAction { get; set; } = LabelIntersectAction.None;
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
		
		public string Height { get; set; }

		public string Width { get; set; }

		public bool IsShowChartTitle { get; set; } = false;

		public string ChartTitle { get; set; }

		public SelectionMode SelectionMode { get; set; } = SelectionMode.Series;

		public bool IsEnableTooltip { get { return TooltipSettings.IsEnable; } set { TooltipSettings.IsEnable = value; } }

		public TooltipConfiguration TooltipSettings { get; set; } = new TooltipConfiguration();

		public SyncfiusionLegendDefinition LegendSettings { get; set; } = new SyncfiusionLegendDefinition();

		public ChartSeriesType ChartSeriesType { get; set; } = ChartSeriesType.StackingColumn;

		public bool IsShowMarker { get; set; } = false;

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

		public ChartAreaBorder Border { get; set; } = new ChartAreaBorder();

		public ReportSyncfusionBaseChartWidget()
		{
		}
	}
}
