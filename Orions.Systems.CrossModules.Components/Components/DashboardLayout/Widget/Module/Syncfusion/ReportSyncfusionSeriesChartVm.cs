using Orions.Common;
using Orions.Infrastructure.Reporting;
using Syncfusion.EJ2.Blazor.Charts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(ReportSyncfusionSeriesChartWidget))]
	public class ReportSyncfusionSeriesChartVm : ReportWidgetVm<ReportSyncfusionSeriesChartWidget>
	{
		protected override bool AutoRegisterInDashboard => true;

		public ReportSyncfusionSeriesChartVm()
		{
		}

		public void HandlePointOnclick(IPointEventArgs args)
		{
			var series = this.ReportChartData.Series[(int)args.SeriesIndex];
			var category = series.Name;
			var currentData = series.Data[(int)args.PointIndex];

			if (this.Widget.InteractiveModeCategoryFiltering)
			{
				this.DashboardVm.SetStringFilters(this.Widget.FilterGroup, new string[] { category }, ReportInstruction.Targets.Column);
			}

			if (this.Widget.InteractiveModeDateTimeFiltering)
			{
				if (series.Data.Count <= 1)
					return;

				if (series.Data.Count == args.PointIndex + 1)
				{
					var prevData = series.Data[(int)args.PointIndex - 1];
					//var prevDif = currentData.DatePosition - prevData.DatePosition;
					//= category;
					this.DashboardVm.SetDateTimeFilters(this.Widget.FilterGroup, prevData.DatePosition, currentData.DatePosition, ReportInstruction.Targets.Column);
				}
				else
				{
					var nextData = series.Data[(int)args.PointIndex + 1];
					//= category;
					//var nextDif = nextData.DatePosition - currentData.DatePosition;
					this.DashboardVm.SetDateTimeFilters(this.Widget.FilterGroup, currentData.DatePosition, nextData.DatePosition, ReportInstruction.Targets.Column);
				}
			}

			if (this.Widget.InteractiveModeCategoryFiltering || this.Widget.InteractiveModeDateTimeFiltering)
				this.DashboardVm.UpdateDynamicWidgetsAsync();
		}
	}
}
