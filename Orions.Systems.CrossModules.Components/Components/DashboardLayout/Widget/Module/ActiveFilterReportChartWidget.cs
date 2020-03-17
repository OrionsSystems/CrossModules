using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ActiveFilterReportChartWidget : ReportChartWidget
	{
		//[HelpText("This widget can serve as a source for applying filtering and period display rules")]
		//public bool AllowFiltrationSource { get; set; } = false;

		[HelpText("This widget can serve as a target for active filtering and period display rules")]
		public bool AllowFiltrationTarget { get; set; } = true;

		public bool AllowDynamicFiltration { get; set; }

		public ActiveFilterReportChartWidget()
		{
		}
	}
}
