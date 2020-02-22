using Orions.Common;
using Orions.Infrastructure.Reporting;

using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class WidgetDataSource : UnifiedBlob
	{
		public DataSourceNameMapping Mapping { get; set; } = new DataSourceNameMapping();

		public IconMapping IconMapping { get; set; } = new IconMapping();

		[HelpText("Include categories by enter comma names")]
		public string IncludeCategories { get; set; }

		[HelpText("Exclude categories by enter comma names")]
		public string ExcludeCategories { get; set; }

		[HelpText("Remove prefix from all categories if find it")]
		public string ExcludePrefix { get; set; }

		public WidgetDataSource()
		{
		}

		public async Task<Report[]> GenerateFilteredReportResultAsync(WidgetDataSourceContext context)
		{
			var result = await OnGenerateFilteredReportResultAsync(context);

			if (this.Mapping != null)
				result?.MapNames(this.Mapping);


			if (!string.IsNullOrWhiteSpace(this.ExcludePrefix))
			{
				foreach (var col in result.ColumnsDefinitions ?? Enumerable.Empty<ReportColumnTemplate>())
				{
					col.Title = col.Title.Replace(this.ExcludePrefix, "");
				}
			}

			return new Report[] { result };
		}

		protected abstract Task<Report> OnGenerateFilteredReportResultAsync(WidgetDataSourceContext context);
	}
}
