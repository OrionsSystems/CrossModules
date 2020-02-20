using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
   public class NumberCardWidget : ReportChartWidget, IDashboardWidget
   {
      public SeparatorConfiguration SepratorsSettings { get; set; } = new SeparatorConfiguration() { Height = 1 };

      public List<CardItem> CustomItems { get; set; } = new List<CardItem>();

      public bool IsShowCardIcons { get; set; }

      public bool MyProperty { get; set; }

      public NumberCardWidget()
      {
         this.Label = "Number Card";
      }
   }
}
