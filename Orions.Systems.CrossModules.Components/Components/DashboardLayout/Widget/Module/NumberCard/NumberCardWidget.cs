using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
   public class NumberCardWidget : ActiveFilterReportChartWidget
   {
      public SeparatorConfiguration SepratorsSettings { get; set; } = new SeparatorConfiguration() { Height = 1 };

      public List<CardItem> CustomItems { get; set; } = new List<CardItem>();

      public bool IsShowCardIcons { get; set; }

      public bool ShowElementTitle { get; set; } = true;

      public NumberCardWidget()
      {
         this.Label = "Number Card";
      }

      public override string GetIcon()
      {
         return "<svg version=\"1.1\" class=\"icon-grid\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" " +
            "viewBox=\"0 0 150 150\" style=\"enable-background:new 0 0 150 150;\" xml:space=\"preserve\"> " +
            "<path class=\"icon-grid-item\" d=\"M8.1,51.8h29.6V22.3H8.1V51.8z M15.9,30.1h13.9V44H15.9V30.1z\"/> " +
            "<path class=\"icon-grid-item\" d=\"M47.1,22.3v29.6h94.7V22.3H47.1z M133.9,44h-79V30.1h79V44z\"/> " +
            "<path class=\"icon-grid-item\" d=\"M8.1,90.2h29.6V60.6H8.1V90.2z M15.9,68.5h13.9v13.9H15.9V68.5z\"/> " +
            "<path class=\"icon-grid-item\" d=\"M47.1,90.2h94.7V60.6H47.1V90.2z M54.9,68.5h79v13.9h-79V68.5z\"/> " +
            "<path class=\"icon-grid-item\" d=\"M8.1,128.6h29.6V99H8.1V128.6z M15.9,106.9h13.9v13.9H15.9V106.9z\"/> " +
            "<path class=\"icon-grid-item\" d=\"M47.1,128.6h94.7V99H47.1V128.6z M54.9,106.9h79v13.9h-79V106.9z\"/> </svg>";
      }
   }
}
