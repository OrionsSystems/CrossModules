using Orions.Common;
using Orions.Infrastructure.Reporting;
using Syncfusion.EJ2.Blazor.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(ReportSyncfusionSeriesChartWidget))]
	public class ReportSyncfusionSeriesChartVm : ReportWidgetVm<ReportSyncfusionSeriesChartWidget>
	{
		protected override bool AutoRegisterInDashboard => true;

		public ReportSyncfusionSeriesChartVm()
		{
		}

		public async Task HandlePointOnclick(IPointEventArgs args)
		{
			//if (this.Widget.AllowFiltrationSource == false)
			//	return;

			var series = this.ReportChartData.Series[(int)args.SeriesIndex];
			var category = series.Name;
			ReportSeriesChartDataItem currentData = series.Data[(int)args.PointIndex];

			var data = this.DashboardVm.ObtainFilterGroup(this.Widget);

			bool didModify = false;
			if (this.Widget.AllowFiltrationSource_TextCategory)
			{
				data.FilterLabels = new string[] { category };
				data.FilterTarget = ReportFilterInstruction.Targets.Column;

				didModify = true;
			}

			if (this.Widget.AllowFiltrationSource_DateTime && series.Data.Count > 1)
			{
				didModify = true;

				data.StartTime = currentData.StartTime;
				data.EndTime = currentData.EndTime;

				//if (series.Data.Count == args.PointIndex + 1)
				//{// Last one
				//	var prevData = series.Data[(int)args.PointIndex - 1];

				//	var delta = currentData.DatePosition - prevData.DatePosition;

				//	data.StartTime = currentData.DatePosition;
				//	data.EndTime = currentData.DatePosition + delta;

				//	didModify = true;
				//}
				//else
				//{// First or a middle one.
				//	var nextData = series.Data[(int)args.PointIndex + 1];

				//	data.StartTime = currentData.DatePosition;
				//	data.EndTime = nextData.DatePosition;

				//	didModify = true;
				//}
			}

			if (didModify)
				await this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();
		}

		public void SeparateSeriesCategory()
		{
			if (ReportChartData.Series.Count != 1) return;

			var ser = ReportChartData.Series.FirstOrDefault();

			var series = ser.Data.Select(it => new ReportSeriesChartData() { Data = new List<ReportSeriesChartDataItem> { it }, Name = it.Label }).ToList();

			//var series = ser.Data.Select(it => new ReportSeriesChartData() { Data = ser.Data.Select(d => new ReportSeriesChartDataItem() { 
			//	Label = d.Label, 
			//	Value = d.Label == it.Label ? d.Value : String.Empty, 
			//	ColumnTemplate = d.ColumnTemplate, 
			//	RowTemplate = d.RowTemplate
			//}).ToList(), Name = it.Label }).ToList();

			ReportChartData.Clean();
			ReportChartData.Series.AddRange(series);
			//ReportChartData.AddCategoryRange(series.Select(it => it.Name).ToArray());
		}
	}
}
