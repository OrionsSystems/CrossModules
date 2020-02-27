using System;

namespace Orions.Systems.CrossModules.Components
{
   public class ReportSyncfusionSeriesChartWidget : ReportSyncfusionBaseChartWidget
   {
      public ReportSyncfusionSeriesChartWidget()
      {
         this.Label = "Series Chart";
      }

      public override string GetIcon()
      {
         return "<svg version=\"1.1\" class=\"icon-line-chart\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 150 150\" style=\"enable-background:new 0 0 150 150;\" xml:space=\"preserve\"> " +
            "<path class=\"icon-line-chart-item-1\" d=\"M19.4,133.5h17.1c1.3,0,2.5-1.1,2.5-2.5V92l-12.4,12.4c-2.4,2.4-5.5,3.9-8.7,4.3v24.1 C18.2,133.2,18.8,133.5,19.4,133.5z\"/>" +
            "<path class=\"icon-line-chart-item-2\" d=\"M47.5,91.7V131c0,1.3,1.1,2.5,2.5,2.5h17.1c1.3,0,2.5-1.1,2.5-2.5v-23.1c-3.9-0.1-7.6-1.6-10.3-4.4L47.5,91.7z\"/>" +
            "<path class=\"icon-line-chart-item-3\" d=\"M78.1,105.4V131c0,1.3,1.1,2.5,2.5,2.5h17.1c1.3,0,2.5-1.1,2.5-2.5V83.9l-19.6,19.6 C79.8,104.2,79,104.9,78.1,105.4z\"/>" +
            "<path class=\"icon-line-chart-item-4\" d=\"M129.1,54.9l-20.4,20.4V131c0,1.3,1.1,2.5,2.5,2.5h17.1c1.3,0,2.5-1.1,2.5-2.5V56.4c-0.6-0.5-1-0.9-1.3-1.2 L129.1,54.9z\"/>" +
            "<path class=\"icon-line-chart-item-5\" d=\"M143.1,18.7c-0.8-0.9-2-1.3-3.6-1.3c-0.1,0-0.3,0-0.4,0c-8,0.4-15.9,0.8-23.9,1.1c-1.1,0.1-2.5,0.1-3.7,1.3 c-0.4,0.4-0.6,0.8-0.9,1.3c-1.2,2.6,0.5,4.2,1.3,5l2,2c1.4,1.4,2.8,2.8,4.2,4.2L69.9,80.5L48.3,58.8c-1.3-1.3-3-2-4.9-2 " +
            "c-1.9,0-3.6,0.7-4.9,2L17.8,79.5v20.9c1.1-0.3,2.1-0.9,2.9-1.7L43.4,76L65,97.7c1.3,1.3,3,2,4.9,2s3.6-0.7,4.9-2l54.3-54.3l6.1,6.1 " +
            "c0.7,0.7,1.7,1.7,3.3,1.7c0.6,0,1.3-0.2,2-0.5c0.4-0.3,0.8-0.5,1.2-0.9c1.2-1.2,1.4-2.8,1.5-4c0.2-5.2,0.5-10.3,0.7-15.5l0.4-7.8 C144.4,20.9,144,19.6,143.1,18.7z\"/>" +
            "<path class=\"icon-line-chart-item-0\" d=\"M16.9,131c0,0.8,0.3,1.4,0.9,1.9v-24.1c-0.3,0-0.6,0.1-0.9,0.1V131z\"/></svg>";
      }
   }
}
