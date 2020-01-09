using System;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportSyncfusionSeriesChartWidget : ReportSyncfusionBaseChartWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Stacked Series Chart (Syncfusion)";

        public override Type GetViewComponent()
        {
            return typeof(ReportSyncfusionSeriesChart);
        }
    }
}
