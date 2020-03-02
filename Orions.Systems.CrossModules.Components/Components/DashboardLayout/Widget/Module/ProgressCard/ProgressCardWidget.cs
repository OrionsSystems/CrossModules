using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
   public class ProgressCardWidget : ActiveFilterReportChartWidget
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

      public override string GetIcon()
      {
         return "<svg version=\"1.1\" class=\"icon-progress\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\"  viewBox=\"0 0 150 150\" style=\"enable-background:new 0 0 150 150;\" xml:space=\"preserve\">" +
            "<path class=\"icon-progress-item\" d=\"M45.2,40.2c8.5,0,15.5-6.9,15.5-15.5c0-8.5-6.9-15.5-15.5-15.5c-8.5,0-15.5,6.9-15.5,15.5 C29.7,33.3,36.7,40.2,45.2,40.2z M45.2,19.6c2.8,0,5.2,2.3,5.2,5.2c0,2.8-2.3,5.2-5.2,5.2c-2.8,0-5.2-2.3-5.2-5.2 C40.1,21.9,42.4,19.6,45.2,19.6z " +
            "M89.9,9.2l13.6,21L89.7,50.6h41v50.9H110v10.1H88.6v9.8H77.5V93.9l-17.6-7.7V67.8l8.9,8h20.6V65.5 H72.8L55.9,50.2c-2.8-3.1-6.9-5.1-11.5-5.1H32.6v0c-4.9,0.3-9.3,2.8-12,6.9L8.8,69.6v22h10.3V72.8l8.3-12.4v52.5l-14.7,23.3l8.7,5.5 " +
            "l16.3-25.8V55.5h6.7c1.2,0,2.4,0.4,3.3,1.2l0,0.1l0.8,0.7c0.7,0.9,1.1,2,1.1,3.2v32.3l17.6,7.7v30.5H48.3v10.3h29.2v-9.8h21.4v-9.8 h21.4v-10.1H141V9.2H89.9z " +
            "M109.2,40.2l6.7-9.9l-7-10.8h21.7v20.7H109.2z\"/></svg>";
      }
   }
}
