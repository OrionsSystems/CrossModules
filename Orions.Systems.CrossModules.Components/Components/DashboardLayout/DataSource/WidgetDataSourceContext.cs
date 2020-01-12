using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	/// <summary>
	/// Contains all informated relevant to executing a Widget data source operation.
	/// </summary>
	public class WidgetDataSourceContext
	{
		public UniFilterData DynamicFilter { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public ILogger Logger { get; set; } = Orions.Common.Logger.Instance;

		public WidgetDataSourceContext()
		{
		}
	}
}
