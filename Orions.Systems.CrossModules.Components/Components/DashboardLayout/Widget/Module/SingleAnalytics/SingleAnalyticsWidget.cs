using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
   public class SingleAnalyticsWidget : ReportChartWidget, IDashboardWidget
   {
      public List<SingleAnalyticsItem> CustomItems { get; set; } = new List<SingleAnalyticsItem>();

      public bool IsShowCardIcons { get; set; }

      public SingleAnalyticsWidget()
      {
         this.Label = "Single Analytics Widget";
      }
   }
}
