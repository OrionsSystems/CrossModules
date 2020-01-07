using System;
using Telerik.Blazor;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportTelericColumChartWidget : ReportTelericBaseChartWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Column Chart (Teleric)";

        public override ChartSeriesType ChartSeriesType { get; set; } = ChartSeriesType.Column;

        public override Type GetViewComponent()
        {
            return typeof(ReportTelericColumChart);
        }
    }
}
