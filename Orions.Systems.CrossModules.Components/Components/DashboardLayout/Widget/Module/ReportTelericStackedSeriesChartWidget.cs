using System;
using Telerik.Blazor;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportTelericStackedSeriesChartWidget : ReportTelericBaseChartWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Stacked Series Chart (Teleric)";

        public override ChartSeriesType ChartSeriesType { get; set; } = ChartSeriesType.Column;

        public override bool IsChartSeriesStackEnabled { get; set; } = true;

        public override Type GetViewComponent()
        {
            return typeof(ReportTelericStackedSeriesChart);
        }
    }
}
