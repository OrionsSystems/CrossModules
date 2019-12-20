using System;

namespace Orions.Systems.CrossModules.Components
{
    public class DemoSmartChartWidget : DashboardWidgetBase, IDashboardWidget
    {
        public override string Label { get; set; } = "Demo Smart Chart";

        public override Type GetViewComponent()
        {
            return typeof(DemoSmartChart);
        }
    }
}
