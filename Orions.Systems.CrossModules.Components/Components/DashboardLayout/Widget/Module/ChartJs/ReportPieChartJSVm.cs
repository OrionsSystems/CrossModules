using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(ReportPieChartJSWidget))]
	public class ReportPieChartJSVm : ReportWidgetVm<ReportPieChartJSWidget>
	{
		protected override bool AutoRegisterInDashboard => true;

		public ReportPieChartJSVm()
		{
		}
	}
}
