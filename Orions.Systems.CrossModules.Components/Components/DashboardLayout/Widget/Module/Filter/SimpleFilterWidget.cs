﻿using Orions.Common;
using Orions.Infrastructure.Reporting;
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

      [UniBrowsable(false)]
      public DateTime? StartDate { get; set; }

      [UniBrowsable(false)]
      public DateTime? EndDate { get; set; }

      public bool ShowTextLabelSelection { get; set; } = true;

      public bool ShowDateTimeSelection { get; set; } = true;

      public ReportInstruction.Targets FilterTarget { get; set; }

      public SimpleFilterWidget()
      {
         this.Label = "Simple Filter"; // Title of this widget.
      }
   }
}