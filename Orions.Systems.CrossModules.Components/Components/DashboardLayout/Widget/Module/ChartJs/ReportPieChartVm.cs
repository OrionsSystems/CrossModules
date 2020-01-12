using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(ReportPieChartWidget))]
	public class ReportPieChartVm : ReportWidgetVm<ReportPieChartWidget>
	{
		protected override bool AutoRegisterInDashboard => true;

		public ReportPieChartVm()
		{
		}
	}
}
