using System;

namespace Orions.Systems.CrossModules.Components
{
   public class ReportPieChartWidget : ReportBaseWidget, IDashboardWidget
   {
      public int Height { get; set; } = 400;

      public int Width { get; set; } = 400;

      public ReportPieChartWidget()
      {
         Label = "Report Pie Chart";
      }
   }
}
