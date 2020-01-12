using System;

namespace Orions.Systems.CrossModules.Components
{
   public class SimpleHtmlWidget : DashboardWidgetBase, IDashboardWidget
   {
      [UniBrowsable(UniBrowsableAttribute.EditTypes.MultiLineText)]
      public string RawHtml { get; set; }

      public SimpleHtmlWidget()
      {
         this.Label = "Simple Html Widget";
      }
   }
}
