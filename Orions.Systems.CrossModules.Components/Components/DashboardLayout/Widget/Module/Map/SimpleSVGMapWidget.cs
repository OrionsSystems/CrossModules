using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
   public class SimpleSVGMapWidget : DashboardWidget, IDashboardWidget
   {
      [HelpText("Highlight selected zone")]
      public string Zone { get; set; }

      [HelpText("Add tag to redirect to tagged dashboard")]
      public string Tag { get; set; }

      public SimpleSVGMapWidget()
      {
         this.Label = "Simple SVG Map Widget";
      }
   }
}
