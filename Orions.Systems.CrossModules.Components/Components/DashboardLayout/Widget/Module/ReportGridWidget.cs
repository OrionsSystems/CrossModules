using System;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportGridWidget : DashboardWidgetBase, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Grid";

        public string MetadatasetId { get; set; }

        public string ReportResultId { get; set; }

        public bool ShowTitle { get; set; } = true;

        public bool ShowFooter { get; set; } = true;

        public override Type GetViewComponent()
        {
            return typeof(ReportGrid);
        }
    }
}
