using System;

namespace Orions.Systems.CrossModules.Components
{
   public class ReportPieChartJSWidget : ReportBaseWidget, IDashboardWidget
   {
      public int Height { get; set; } = 400;

      public int Width { get; set; } = 400;

      public ReportPieChartJSWidget()
      {
         Label = "Report Pie ChartJS";
      }
   }
}
