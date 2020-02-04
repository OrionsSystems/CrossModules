using System;
using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
   public class SingleAnalyticsWidget : DashboardWidget, IDashboardWidget
   {
      public class SingleAnalyticsItem 
      {
         public string Title { get; set; }

         public string Value { get; set; }

         public string IconHtml { get; set; }
      }

      public List<SingleAnalyticsItem> Items { get; set; } = new List<SingleAnalyticsItem>();

      public SingleAnalyticsWidget()
      {
         this.Label = "Single Analytics Widget";
      }
   }
}
