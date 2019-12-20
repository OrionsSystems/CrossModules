using System;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportPieChartWidget : ReportBaseWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Pie Chart";

        public int Height { get; set; } = 400;

        public int Width { get; set; } = 400;

        public override Type GetViewComponent()
        {
            return typeof(ReportPieChart);
        }
    }
}
