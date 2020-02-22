using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportChartWidget : ReportBaseWidget
	{
		[HelpText("When you click on a chart, should the click apply a DаteTime filter based on what we clicked to the selected Dashboard filter group")]
		public bool AllowFiltrationSource_DateTime { get; set; } = true;

		[HelpText("When you click on a chart, should the click apply a text-category filter based on what we clicked to the selected Dashboard filter group")]
		public bool AllowFiltrationSource_TextCategory { get; set; } = true;

		public ReportChartWidget()
		{
		}
	}
}
