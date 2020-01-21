using System;

namespace Orions.Systems.CrossModules.Components
{
   public class SocialStatusWidget : DashboardWidget, IDashboardWidget
   {

      public class SocialStatus {
         public int VeryLow { get; set; }
         public int Low { get; set; }
         public int Medium { get; set; }
         public int High { get; set; }
         public int VeryHigh { get; set; }
      }

      public SocialStatus Data { get; set; } = new SocialStatus();

      public SocialStatusWidget()
      {
         this.Label = "Social Status Widget";
      }
   }
}
