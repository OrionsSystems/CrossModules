using System;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportTelericLineChartWidget : ReportBaseWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Line Chart (Teleric)";

        public override Type GetViewComponent()
        {
            return typeof(ReportTelericLineChart);
        }
    }
}
