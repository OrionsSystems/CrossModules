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
		[UniJsonIgnore]
      [UniBrowsable(false)]
      public string[] Filters { get; set; }

      [UniJsonIgnore]
      [UniBrowsable(false)]
      public DateTime? StartDate { get; set; }

      [UniJsonIgnore]
      [UniBrowsable(false)]
      public DateTime? EndDate { get; set; }

      [UniJsonIgnore]
      [UniBrowsable(false)]
      public PeriodDefinition Period { get; set; }

      #endregion

      public DateTime? MinDate { get; set; } = new DateTime(2019, 11, 1);

      public DateTime? MaxDate { get; set; } = new DateTime(2019, 12, 31);

      public bool ShowTextLabelSelection { get; set; } = true;

      public bool ShowDateTimeSelection { get; set; } = true;

      public bool ShowPeriodSelection { get; set; } = true;

      public string[] PredefinedFilters { get; set; } = new string[] { };

      public ReportFilterInstruction.Targets FilterTarget { get; set; }

      public SimpleFilterWidget()
      {
         this.Label = "Simple Filter"; // Title of this widget.
      }
   }
}