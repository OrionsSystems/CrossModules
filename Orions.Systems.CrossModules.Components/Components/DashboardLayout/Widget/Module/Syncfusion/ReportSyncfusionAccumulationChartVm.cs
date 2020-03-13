using Orions.Common;
using Orions.Infrastructure.Reporting;
using Syncfusion.EJ2.Blazor.Charts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(ReportSyncfusionAccumulationChartWidget))]
	public class ReportSyncfusionAccumulationChartVm : ReportWidgetVm<ReportSyncfusionAccumulationChartWidget>
	{
		protected override bool AutoRegisterInDashboard => true;

		public ReportSyncfusionAccumulationChartVm()
		{
		}

		public async Task HandlePointOnclick(IPointEventArgs args)
		{
			var series = this.ReportChartData.Series[(int)args.PointIndex];
			var category = series.Name;
			//var currentData = series.Data[(int)args.PointIndex];

			var data = this.DashboardVm.ObtainFilterGroup(this.Widget);

			data.FilterLabels = new string[] { category };
			data.FilterTarget = ReportFilterInstruction.Targets.Column;

			await this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();

		}
	}
}
