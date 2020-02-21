using Orions.Common;
using Orions.Infrastructure.Reporting;
using System;

namespace Orions.Systems.CrossModules.Components
{
   public class SimpleFilterWidget : ReportBaseWidget
   {
      [HelpText("Design options")]
      public SimpleFilterConfiguration Settings { get; set; } = new SimpleFilterConfiguration();

		#region DATA

		/// <summary>
		/// Persist the previous selection of the user.
		/// </summary>
		[UniBrowsable(false)]
      public string[] Filters { get; set; }

      [UniBrowsable(false)]
      public DateTime? StartDate { get; set; }

      [UniBrowsable(false)]
      public DateTime? EndDate { get; set; }

      [UniBrowsable(false)]
      public PeriodDefinition Period { get; set; }

		#endregion

		public bool ShowTextLabelSelection { get; set; } = true;

      public bool ShowDateTimeSelection { get; set; } = true;

      public ReportFilterInstruction.Targets FilterTarget { get; set; }

      public SimpleFilterWidget()
      {
         this.Label = "Simple Filter"; // Title of this widget.
      }
   }
}