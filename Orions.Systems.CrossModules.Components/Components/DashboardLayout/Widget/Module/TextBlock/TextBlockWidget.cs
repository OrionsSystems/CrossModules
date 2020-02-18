using System;

namespace Orions.Systems.CrossModules.Components
{
   public class TextBlockWidget : DashboardWidget, IDashboardWidget
   {
      [UniBrowsable(UniBrowsableAttribute.EditTypes.MultiLineText)]
      public string RawHtml { get; set; }

      public TextBlockWidget()
      {
         this.Label = "Text Block Widget";
      }
   }
}
