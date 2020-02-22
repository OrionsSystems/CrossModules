using Orions.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	[Compatibility("ReportTelericLineChartWidget")]
	public class ReportTelericSeriesChartWidget : ReportTelericBaseChartWidget
	{
		public ReportTelericSeriesChartWidget()
		{
			Label = "Teleric Series Chart";
		}
	}
}
