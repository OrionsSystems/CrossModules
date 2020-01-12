using System;

namespace Orions.Systems.CrossModules.Components
{
   public class SimpleFilterWidget : ReportBaseWidget
   {
      public string[] Filters { get; set; }

      public SimpleFilterWidget()
      {
         this.Label = "Simple Filter";
      }
   }
}
