using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(ReportTelericSeriesChartWidget))]
	public class ReportTelericSeriesChartVm : ReportWidgetVm<ReportTelericSeriesChartWidget>
	{
		protected override bool AutoRegisterInDashboard => true;

		public ReportTelericSeriesChartVm()
		{
		}
	}
}
