using System;

namespace Orions.Systems.CrossModules.Components
{
   public class BodyTypeWidget : DashboardWidget, IDashboardWidget
   {
      public class BodyType {
         public int Anorexic { get; set; }
         public int Slim { get; set; }
         public int SlimFit { get; set; }
         public int Average { get; set; }
         public int AverageFit { get; set; }
         public int MildOverweight { get; set; }
         public int Overweight { get; set; }
         public int VeryOverweight { get; set; }
      }

      public BodyType Data { get; set; } = new BodyType();

      public BodyTypeWidget()
      {
         this.Label = "Body Type Widget";
      }
   }
}
