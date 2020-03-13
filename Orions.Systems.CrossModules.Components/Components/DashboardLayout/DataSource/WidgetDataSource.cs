using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Reporting;

using System;
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

		[HelpText("Convert categories to uppercase")]
		public bool Uppercase { get; set; } = true;

		public WidgetDataSource()
		{
		}

		public async Task<Report[]> GenerateFilteredReportResultAsync(WidgetDataSourceContext context)
		{
			try
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
			catch (Exception e)
			{
				Logger.Instance.Error(this, nameof(GenerateFilteredReportResultAsync), e);
				return new Report[] { };
			}

		}

		protected abstract Task<Report> OnGenerateFilteredReportResultAsync(WidgetDataSourceContext context);
	}
}
