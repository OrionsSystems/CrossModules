using System;

namespace Orions.Systems.CrossModules.Components
{
   public class TransportationStatsWidget : DashboardWidget, IDashboardWidget
   {

      public class Transportation
      {
         public int Sedan { get; set; }
         public int Hatchback { get; set; }
         public int Convertable { get; set; }
         public int Minivan { get; set; }
         public int Truck { get; set; }
         public int LargerTruck { get; set; }
         public int Motocycle { get; set; }
         public int Bikes { get; set; }
      }

      public Transportation Data { get; set; } = new Transportation();

      public string Color { get; set; } = "#c8c2c2";

      public TransportationStatsWidget()
      {
         this.Label = "Transportation Statistics Widget";
      }

      public int GetAveragePeoplePerCar() 
      {
         //TODO 
         var rnd = new Random();
         return rnd.Next(0, 20);
      }

      public int GetAverageFamilyePerCar()
      {
         //TODO 
         var rnd = new Random();
         return rnd.Next(0, 20);
      }
   }
}
