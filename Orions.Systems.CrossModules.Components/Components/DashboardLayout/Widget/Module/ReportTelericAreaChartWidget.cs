using System;
using Telerik.Blazor;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportTelericAreaChartWidget : ReportTelericBaseChartWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Area Chart (Teleric)";

        public override ChartSeriesType ChartSeriesType { get; set; } = ChartSeriesType.Area;

        public override Type GetViewComponent()
        {
            return typeof(ReportTelericAreaChart);
        }
    }
}
