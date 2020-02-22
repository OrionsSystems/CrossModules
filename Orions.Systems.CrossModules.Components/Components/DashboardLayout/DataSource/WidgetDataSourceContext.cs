using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	/// <summary>
	/// Contains all informated relevant to executing a Widget data source operation.
	/// </summary>
	public class WidgetDataSourceContext
	{
		/// <summary>
		/// Instructions, filtering etc. for the dashboard group we are in.
		/// </summary>
		public DashboardGroupData GroupFilterData { get; set; } 

		public IHyperArgsSink HyperStore { get; set; }

		public ILogger Logger { get; set; } = Orions.Common.Logger.Instance;

		public CancellationToken CancellationToken { get; set; } = default(CancellationToken);

		public WidgetDataSourceContext()
		{
		}
	}
}
