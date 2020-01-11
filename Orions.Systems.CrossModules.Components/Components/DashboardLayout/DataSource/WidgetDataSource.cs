using Orions.Common;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class WidgetDataSource : UnifiedBlob
	{
		public virtual bool SupportsDynamicFiltration => false;

		public WidgetDataSource()
		{
		}

		public abstract Task<IReportResult> GenerateReportResultAsync(WidgetDataSourceContext context);
	}
}
