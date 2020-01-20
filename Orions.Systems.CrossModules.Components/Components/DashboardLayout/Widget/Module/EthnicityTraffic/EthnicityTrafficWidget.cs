namespace Orions.Systems.CrossModules.Components
{
   public class EthnicityTrafficWidget : DashboardWidget, IDashboardWidget
   {

      public class Etnicity {
         public int Caucasian { get; set; }

         public int Asian { get; set; }

         public int AfricanAmerican { get; set; }

         public int Hispanic { get; set; }

         public int NativeAmerican { get; set; }
      }

      public Etnicity Data { get; set; } = new Etnicity();

      public string Color { get; set; } = "#c8c2c2";

      public EthnicityTrafficWidget()
      {
         this.Label = "Ethnicity Traffic Widget";
      }
   }
}
