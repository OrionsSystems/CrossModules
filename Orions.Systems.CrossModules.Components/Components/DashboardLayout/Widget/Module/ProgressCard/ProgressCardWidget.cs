using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
   public class ProgressCardWidget : ReportChartWidget, IDashboardWidget
   {
      public List<CardItem> CustomItems { get; set; } = new List<CardItem>();

      public bool IsShowCardIcons { get; set; }

      public bool IsShowPercentage { get; set; } = true;

      public bool ShowElementTitle { get; set; }

      public string Color { get; set; } = "#c8c2c2";

      public ProgressCardWidget()
      {
         this.Label = "Progress Card";
      }
   }
}
