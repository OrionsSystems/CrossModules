using Orions.Common;
using Orions.Infrastructure.Reporting;

using Syncfusion.EJ2.Blazor.Charts;

using System.Linq;
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
			if (!this.Widget.AllowFiltrationSource_TextCategory) return;

			var series = this.ReportChartData.Series[(int)args.PointIndex];
			var category = series.Name;

			var data = this.DashboardVm.ObtainFilterGroup(this.Widget);

			var filters = new string[] { category.ToUpper() };
			if (data.FilterLabels == null)
			{
				data.FilterLabels = filters;
			}
			else
			{
				data.FilterLabels = data.FilterLabels.Concat(filters).Distinct().ToArray();
			}

			data.FilterTarget = ReportFilterInstruction.Targets.Column;

			if (data.AllowDynamicFiltration)
			{
				await this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();
			}
		}
	}
}
