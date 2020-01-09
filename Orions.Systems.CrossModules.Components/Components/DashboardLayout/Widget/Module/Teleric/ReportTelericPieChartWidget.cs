using System;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportTelericPieChartWidget : ReportBaseWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Pie Chart (Teleric)";

        public override Type GetViewComponent()
        {
            return typeof(ReportTelericPieChart);
        }
    }
}
