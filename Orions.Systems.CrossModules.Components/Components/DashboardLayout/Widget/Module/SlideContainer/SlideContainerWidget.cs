namespace Orions.Systems.CrossModules.Components
{
   public class SlideContainerWidget : DashboardWidget
   {
      public DashboardWidget[] Data { get; set; } = new DashboardWidget[] { };

      public SlideContainerWidget()
      {
         this.Label = "Slide Container";
      }
   }
}
