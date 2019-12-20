using System;

namespace Orions.Systems.CrossModules.Components
{
    public class SimpleHtmlWidget : DashboardWidgetBase, IDashboardWidget
    {
        public override string Label { get; set; } = "Simple Html Widget";

        [UniBrowsable(UniBrowsableAttribute.EditTypes.MultiLineText)]
        public string RawHtml { get; set; }

        public override Type GetViewComponent()
        {
            return typeof(SimpleHtml);
        }
    }
}
