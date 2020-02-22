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
   }
}
