using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(ReportTelericPieChartWidget))]
	public class ReportTelericPieChartVm : ReportWidgetVm<ReportTelericPieChartWidget>
	{
		protected override bool AutoRegisterInDashboard => true;

		public ReportTelericPieChartVm()
		{
		}
	}
}
