namespace Orions.Systems.CrossModules.Components
{
   public class SimpleSVGMapWidget : DashboardWidget, IDashboardWidget
   {
      public string Zone { get; set; }

      public SimpleSVGMapWidget()
      {
         this.Label = "Simple SVG Map Widget";
      }
   }
}
