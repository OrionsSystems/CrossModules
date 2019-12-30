using System;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportTelericBarChartWidget : ReportBaseWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Bar Chart (Teleric)";

        public override Type GetViewComponent()
        {
            return typeof(ReportTelericBarChart);
        }
    }
}
