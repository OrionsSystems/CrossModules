namespace Orions.Systems.CrossModules.Components
{
   public class ReportSyncfusionAccumulationChartWidget : ReportSyncfusionBaseAccumulationChartWidget
   {
      public ReportSyncfusionAccumulationChartWidget()
      {
         this.Label = "Accumulation Chart";
      }

      public override string GetIcon()
      {
         return "<svg version=\"1.1\" class=\"icon-pie-chart\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 150 150\" style=\"enable-background:new 0 0 150 150;\" xml:space=\"preserve\">" +
            "<path class=\"icon-pie-chart-item-0\" d=\"M79.4,70.1h64.4c-2.4-34.5-29.9-62-64.4-64.4V70.1z\"/>" +
            "<path class=\"icon-pie-chart-item-1\" d=\"M125.7,121.9c10.4-11.3,17.1-26,18.2-42.2H83.5L125.7,121.9z\"/>" +
            "<path class=\"icon-pie-chart-item-2\" d=\"M69.8,72.9V5.8C33.8,8.2,5.3,38.2,5.3,74.9c0,17.4,6.4,33.3,17.1,45.5L69.8,72.9z\"/>" +
            "<path class=\"icon-pie-chart-item-3\" d=\"M73.2,83.1l-44.1,44.1c12.2,10.6,28.1,17.1,45.5,17.1c16.7,0,32-5.9,44-15.8L73.2,83.1z\"/></svg>";
      }
   }
}
