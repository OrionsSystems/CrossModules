using Orions.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	[Compatibility("ReportTelericLineChartWidget")]
	public class ReportTelericSeriesChartWidget : ReportTelericBaseChartWidget, IDashboardWidget
	{
		public ReportTelericSeriesChartWidget()
		{
			Label = "Teleric Series Chart";
		}
	}
}
