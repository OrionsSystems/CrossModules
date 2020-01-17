using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportChartWidget : ReportBaseWidget
	{
		[HelpText("When you click on a chart, should the click apply a DAteTime filter based on what we clicked to the selected Dashboard filter group")]
		public bool InteractiveModeDateTimeFiltering { get; set; } = true;

		[HelpText("When you click on a chart, should the click apply a text-category filter based on what we clicked to the selected Dashboard filter group")]
		public bool InteractiveModeCategoryFiltering { get; set; } = true;

		public ReportChartWidget()
		{
		}
	}
}
