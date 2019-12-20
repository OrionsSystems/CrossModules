using System;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportGridWidget : ReportBaseWidget, IDashboardWidget
    {
        public override string Label { get; set; } = "Report Grid";

        public override Type GetViewComponent()
        {
            return typeof(ReportGrid);
        }
    }
}
