using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(SingleAnalyticsWidget))]
	public class SingleAnalyticsVm : ReportWidgetVm<SingleAnalyticsWidget>
	{
		public SingleAnalyticsVm()
		{
		}
	}
}
