using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(SimpleHtmlWidget))]
	public class SimpleHtmlVm : WidgetVm<SimpleHtmlWidget>
	{
		public SimpleHtmlVm()
		{
		}
	}
}
