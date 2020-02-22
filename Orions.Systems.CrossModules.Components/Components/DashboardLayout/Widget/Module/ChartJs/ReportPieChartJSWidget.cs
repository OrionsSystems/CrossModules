using System;

namespace Orions.Systems.CrossModules.Components
{
   public class ReportPieChartJSWidget : ReportBaseWidget
   {
      public int Height { get; set; } = 400;

      public int Width { get; set; } = 400;

      public ReportPieChartJSWidget()
      {
         Label = "Pie Chart JS";
      }
   }
}
