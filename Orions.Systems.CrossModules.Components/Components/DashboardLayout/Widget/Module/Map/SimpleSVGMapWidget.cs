using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
   public class SimpleSVGMapWidget : DashboardWidget
   {
      [HelpText("Selected zone")]
      public string Zone { get; set; }

      [HelpText("Add tag to redirect to tagged dashboard")]
      public string Tag { get; set; }

      [HelpText("Selected zone color")]
      public string ZoneColor { get; set; } = "#159C70";

      [HelpText("Highlight zone color")]
      public string ZoneColorOver { get; set; } = "#13513A";

      public SimpleSVGMapWidget()
      {
         this.Label = "Simple Map";
      }

      public override string GetIcon()
      {
         return "<svg version=\"1.1\" class=\"icon-map\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\"   " +
            "viewBox=\"0 0 150 150\" style=\"enable-background:new 0 0 150 150;\" xml:space=\"preserve\"> " +
            "<g>  <path class=\"icon-map-item-0\" d=\"M145.3,135.9l-11.1-61c-0.1-0.6-0.5-1.1-1-1.4L105.4,58c-0.4-0.2-0.8-0.3-1.2-0.3l5.1,65.4l-0.2,0.1l33.3,15   " +
            "c0.3,0.1,0.6,0.2,0.8,0.2c0.5,0,0.9-0.1,1.3-0.4C145.2,137.4,145.4,136.6,145.3,135.9z\"/>  " +
            "<path class=\"icon-map-item-0\" d=\"M74.9,72.8L46.4,57.9c-0.3-0.2-0.7-0.3-1.1-0.2c0.1,0,0.2,0,0.3,0l-5.1,65.4l33.5,15c0.3,0.1,0.6,0.2,0.8,0.2   l0,0V72.8z\"/> </g> " +
            "<g>  <path class=\"icon-map-item-1\" d=\"M45.6,57.7c-0.4,0-0.8,0-1.2,0.3L16.6,73.5c-0.5,0.3-0.9,0.8-1,1.4l-11.1,61c-0.1,0.8,0.2,1.5,0.8,2   " +
            "c0.4,0.3,0.8,0.4,1.3,0.4c0.3,0,0.6-0.1,0.8-0.2l33.2-14.9L45.6,57.7z\"/>  " +
            "<path class=\"icon-map-item-1\" d=\"M104.2,57.7c-0.3,0-0.5,0.1-0.8,0.2L74.9,72.8v65.5c0,0,0,0,0,0c0.3,0,0.6-0.1,0.8-0.2l33.5-15L104.2,57.7z\"/> </g> " +
            "<polygon class=\"icon-map-item-2\" points=\"12.2,93.4 10,105.8 43.3,88.2 44,78.2 44.2,76.6 \"/> " +
            "<path class=\"icon-map-item-3\" d=\"M43.6,84.3l-0.3,3.9L10,105.8l-5.5,30.1c-0.1,0.8,0.2,1.5,0.8,2c0.4,0.3,0.8,0.4,1.3,0.4c0.3,0,0.6-0.1,0.8-0.2  l33.2-14.9L43.6,84.3L43.6,84.3z\"/> " +
            "<g>  <polygon class=\"icon-map-item-4\" points=\"74.9,81 44.2,76.6 43.3,88.2 74.9,136.2 74.9,118.8 56,89 74.9,91.3  \"/>  " +
            "<polygon class=\"icon-map-item-4\" points=\"131,72.2 117.6,64.8 105,67.8 105.1,69.8 105.8,77.9  \"/> </g> " +
            "<polygon class=\"icon-map-item-2\" points=\"106.9,92.4 105.7,76.9 105.7,76.9 105,67.8 74.9,81 74.9,91.3 94.6,82.8 \"/> " +
            "<path class=\"icon-map-item-4\" d=\"M145.3,135.9l-2.8-15.3l-36.8-42.7l1.1,14.5l37.7,45.5C145.2,137.4,145.4,136.6,145.3,135.9z\"/> " +
            "<path class=\"icon-map-item-5\" d=\"M75,12.6c-15.9,0-28.9,13-28.9,28.9c0,9.9,4.7,20.4,13.9,31.5c6.8,8.1,13.5,13.3,13.7,13.5  " +
            "c0.4,0.3,0.8,0.4,1.3,0.4c0.4,0,0.9-0.1,1.3-0.4c0.3-0.2,7-5.4,13.7-13.5c9.2-11,13.9-21.6,13.9-31.5C104,25.5,91,12.6,75,12.6z   " +
            "M85.8,41.5c0,5.9-4.8,10.7-10.7,10.7s-10.7-4.8-10.7-10.7S69.1,30.8,75,30.8C81,30.8,85.8,35.5,85.8,41.5z\"/> " +
            "<path class=\"icon-map-item-6\" d=\"M75,12.6c-1.3,0-2.5,0.1-3.7,0.2c14.2,1.8,25.2,14,25.2,28.7c0,9.9-4.7,20.4-13.9,31.5  c-4.4,5.2-8.7,9.2-11.3,11.5c1.4,1.3,2.4,2,2.5,2c0.4,0.3,0.8,0.4,1.3,0.4c0.4,0,0.9-0.1,1.3-0.4c0.3-0.2,7-5.4,13.7-13.5  " +
            "c9.2-11,13.9-21.6,13.9-31.5C104,25.5,91,12.6,75,12.6z\"/> " +
            "<path class=\"icon-map-item-2\" d=\"M75.8,138.1l12.5-5.6l-13.3-13.8v19.5c0,0,0,0,0,0C75.2,138.3,75.5,138.2,75.8,138.1z\"/> " +
            "<g>  <path class=\"icon-map-item-7\" d=\"M134.2,74.9c-0.1-0.6-0.5-1.1-1-1.4l-2.2-1.2l-25.3,5.6l0,0l36.8,42.7L134.2,74.9z\"/>  " +
            "<path class=\"icon-map-item-7\" d=\"M74.9,138.3C74.9,138.3,74.9,138.3,74.9,138.3v-2.1L43.3,88.2l-2.7,34.9l33.5,15 C74.3,138.2,74.6,138.3,74.9,138.3z\"/> </g> " +
            "</svg>";
      }
   }
}
