using System;
using Telerik.Blazor;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportTelericBarChartWidget : ReportTelericBaseChartWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Bar Chart (Teleric)";

        public override ChartSeriesType ChartSeriesType { get; set; } = ChartSeriesType.Bar;

        public override Type GetViewComponent()
        {
            return typeof(ReportTelericBarChart);
        }
    }
}
