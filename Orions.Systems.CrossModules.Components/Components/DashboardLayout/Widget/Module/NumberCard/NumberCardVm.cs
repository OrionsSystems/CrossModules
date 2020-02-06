using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(NumberCardWidget))]
	public class NumberCardVm : ReportWidgetVm<NumberCardWidget>
	{
		public NumberCardVm()
		{
		}
	}
}
