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

		public DataSourceNameMapping Mapping { get; set; } = new DataSourceNameMapping();

		public WidgetDataSource()
		{
		}

		public async Task<IReportResult> GenerateFilteredReportResultAsync(WidgetDataSourceContext context)
		{
			var result = await OnGenerateFilteredReportResultAsync(context);

			if (this.Mapping != null)
				result?.MapNames(this.Mapping);

			return result;
		}

		protected abstract Task<IReportResult> OnGenerateFilteredReportResultAsync(WidgetDataSourceContext context);
	}
}
