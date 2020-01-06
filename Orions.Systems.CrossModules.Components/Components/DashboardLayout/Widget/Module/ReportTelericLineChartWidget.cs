using System;
using Telerik.Blazor;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportTelericLineChartWidget : ReportTelericBaseChartWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Line Chart (Teleric)";

        public override ChartSeriesType ChartSeriesType { get; set; } = ChartSeriesType.Line;

        public override Type GetViewComponent()
        {
            return typeof(ReportTelericLineChart);
        }
    }
}
