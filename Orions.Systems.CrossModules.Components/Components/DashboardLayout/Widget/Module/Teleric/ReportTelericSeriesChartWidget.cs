using System;
using Telerik.Blazor;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportTelericSeriesChartWidget : ReportTelericBaseChartWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Teleric Report Series Chart";

        public override Type GetViewComponent()
        {
            return typeof(ReportTelericSeriesChart);
        }
    }
}
