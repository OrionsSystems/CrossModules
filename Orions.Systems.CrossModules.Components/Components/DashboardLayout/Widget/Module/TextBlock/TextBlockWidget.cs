using System;

namespace Orions.Systems.CrossModules.Components
{
   public class TextBlockWidget : DashboardWidget, IDashboardWidget
   {
      [UniBrowsable(UniBrowsableAttribute.EditTypes.MultiLineText)]
      public string RawHtml { get; set; }

      public TextBlockWidget()
      {
         this.Label = "Text Block";
      }

      public override string GetIcon()
      {
         return "<svg version=\"1.1\" class=\"icon-text-block\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" " +
            "x=\"0px\" y=\"0px\" viewBox=\"0 0 150 150\"  style=\"enable-background:new 0 0 150 150;\" xml:space=\"preserve\"> " +
            "<polyline class=\"icon-text-block-item\" points=\"18.9,39 31.8,11.1 128.4,11.1 140.7,37.7 \"/>" +
            "<line class=\"icon-text-block-item\" x1=\"65.2\" y1=\"137.8\" x2=\"94\" y2=\"137.8\"/>" +
            "<line class=\"icon-text-block-item\" x1=\"79.6\" y1=\"11.1\" x2=\"79.6\" y2=\"137.8\"/> " +
            "<polygon class=\"icon-text-block-item\" points=\"20.2,63.2 33.3,83.3 6.7,83.3 \"/> " +
            "<line class=\"icon-text-block-item\" x1=\"20\" y1=\"137.8\" x2=\"20\" y2=\"83.3\"/> </svg>"; 
      }
   }
}
