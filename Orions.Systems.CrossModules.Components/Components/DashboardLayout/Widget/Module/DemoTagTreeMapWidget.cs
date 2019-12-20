using System;

namespace Orions.Systems.CrossModules.Components
{
    public class DemoTagTreeMapWidget : DashboardWidgetBase, IDashboardWidget
    {
        public override string Label { get; set; } = "Demo Tag Tree Map";

        public override Type GetViewComponent()
        {
            return typeof(DemoTagTreeMap);
        }
    }
}
