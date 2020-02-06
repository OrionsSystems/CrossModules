using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
   public class NumberCardWidget : ReportChartWidget, IDashboardWidget
   {
      public List<CardItem> CustomItems { get; set; } = new List<CardItem>();

      public bool IsShowCardIcons { get; set; }

      public NumberCardWidget()
      {
         this.Label = "Number Card Widget";
      }
   }
}
