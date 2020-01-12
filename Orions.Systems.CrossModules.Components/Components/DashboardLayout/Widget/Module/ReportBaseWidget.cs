using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportBaseWidget : DashboardWidget
	{
		[HelpText("Add the data for this widget to use")]
		public WidgetDataSource DataSource { get; set; } = new ReportResultWidgetDataSource();

		[HelpText("Add category filters separated by comma")]
		public string CategoryFilter { get; set; }

		public ReportBaseWidget()
		{
		}
	}
}
