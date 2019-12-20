using System;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportPieChartWidget : DashboardWidgetBase, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Pie Chart";

        public string MetadatasetId { get; set; }

        public string ReportResultId { get; set; }

        public bool ShowTitle { get; set; } = true;

        public bool ShowFooter { get; set; } = true;

        public override Type GetViewComponent()
        {
            return typeof(ReportPieChart);
        }
    }
}
