using Syncfusion.EJ2.Blazor;
using Syncfusion.EJ2.Blazor.Charts;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportSyncfusionBaseChartWidget : ReportChartWidget, IDashboardWidget
	{
		public class LegendDefinition
		{
			public bool Visible { get; set; } = true;
			public LegendPosition Position { get; set; } = LegendPosition.Top;
			public Alignment Alignment { get; set; } = Alignment.Far;

		}
		public bool IsShowChartTitle { get; set; } = false;

		public string ChartTitle { get; set; }

		public string LabelFormat { get; set; } = "dd MMM HH:mm";

		public SelectionMode SelectionMode { get; set; } = SelectionMode.Series;
		public string Skeleton { get; set; } = "Ed";

		public EdgeLabelPlacement EdgeLabelPlacement { get; set; } = EdgeLabelPlacement.Shift;

		public IntervalType IntervalType { get; set; } = IntervalType.Auto;

		public LabelPlacement LabelPlacement { get; set; } = LabelPlacement.OnTicks;

		public bool XAxisIsIndexed { get; set; } = true;

		public bool IsEnableTooltip { get; set; } = true;

		public LegendDefinition LegendSettings { get; set; } = new LegendDefinition();

		public ChartSeriesType ChartSeriesType { get; set; } = ChartSeriesType.StackingColumn;

		public bool IsShowMarker { get; set; } = false;

		public string ChartBackground { get; set; } = "transparent";

		public bool EnableMouseWheelZooming { get; set; }

		public bool EnablePinchZooming { get; set; }

		public bool EnableSelectionZooming { get; set; }

		public bool EnablePan { get; set; }

		public bool EnableScrollbarZooming { get; set; }

		public bool IsEnableCrosshair { get; set; }

		public ReportSyncfusionBaseChartWidget()
		{
		}
	}
}
