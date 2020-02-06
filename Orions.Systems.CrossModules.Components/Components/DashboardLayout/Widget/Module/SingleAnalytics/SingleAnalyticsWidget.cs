using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
   public class SingleAnalyticsWidget : ReportChartWidget, IDashboardWidget
   {
      public List<CardItem> CustomItems { get; set; } = new List<CardItem>();

      public bool IsShowCardIcons { get; set; }

      public SingleAnalyticsWidget()
      {
         this.Label = "Single Analytics Widget";
      }
   }
}
