using Syncfusion.EJ2.Blazor;
using Syncfusion.EJ2.Blazor.Charts;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportSyncfusionBaseChartWidget : ReportBaseWidget, IDashboardWidget
	{
		public bool IsShowChartTitle { get; set; } = false;
		public string ChartTitle { get; set; }

		public string LabelFormat { get; set; } = "Md hm";


		public SelectionMode SelectionMode { get; set; } = SelectionMode.Series;
		public string Skeleton { get; set; } = "Ed";

		public EdgeLabelPlacement EdgeLabelPlacement { get; set; } = EdgeLabelPlacement.Shift;

		public IntervalType IntervalType { get; set; } = IntervalType.Auto;

		public LabelPlacement LabelPlacement { get; set; } = LabelPlacement.OnTicks;

		public bool XAxisIsIndexed { get; set; } = true;

		public bool IsEnableTooltip { get; set; } = true;

		public bool IsEnableLegend { get; set; } = true;

		public ChartSeriesType ChartSeriesType { get; set; } = ChartSeriesType.Column;

		public bool IsShowMarker { get; set; } = true;

		public string ChartBackground { get; set; } = "transparent";

		public LegendPosition LegendPosition { get; set; } = LegendPosition.Bottom;

		public Alignment LegendAlignment { get; set; } = Alignment.Center;

		public bool EnableMouseWheelZooming { get; set; } = true;

		public bool EnablePinchZooming { get; set; } = true;

		public bool EnableSelectionZooming { get; set; } = true;

		public bool EnablePan { get; set; }

		public bool EnableScrollbarZooming { get; set; } = true;

		public bool IsEnableCrosshair { get; set; } = true;
	}
}
