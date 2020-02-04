using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(SingleAnalyticsWidget))]
	public class SingleAnalyticsVm : WidgetVm<SingleAnalyticsWidget>
	{
		public SingleAnalyticsVm()
		{
		}
	}
}
