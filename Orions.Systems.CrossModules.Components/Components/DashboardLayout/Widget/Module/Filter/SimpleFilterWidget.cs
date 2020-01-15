using Orions.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
   public class SimpleFilterWidget : ReportBaseWidget
   {
      /// <summary>
      /// Persist the previous selection of the user.
      /// </summary>
      [UniBrowsable(false)]
      public string[] Filters { get; set; }

      public bool ShowTextLabelSelection { get; set; } = true;

      public bool ShowDateTimeSelection { get; set; } = true;

      public SimpleFilterWidget()
      {
         this.Label = "Simple Filter"; // Title of this widget.
      }
   }
}
