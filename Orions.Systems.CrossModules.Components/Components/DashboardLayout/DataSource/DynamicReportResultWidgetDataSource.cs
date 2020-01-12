using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	/// <summary>
	/// A data source based off of a Report result.
	/// </summary>
	public class DynamicReportResultWidgetDataSource : WidgetDataSource
	{
		[HelpText("The Report to use as a template for this source", HelpTextAttribute.Priorities.Mandatory)]
		[HyperDocumentId.DocumentType(typeof(HyperMetadataReport))]
		public HyperDocumentId? ReportId { get; set; }

		[HelpText("The Metadata set to use as a source", HelpTextAttribute.Priorities.Mandatory)]
		[HyperDocumentId.DocumentType(typeof(HyperMetadataSet))]
		public HyperDocumentId? MetadataSetId { get; set; }

		public override bool SupportsDynamicFiltration => true;

		public DynamicReportResultWidgetDataSource()
		{
		}

		public override async Task<IReportResult> GenerateReportResultAsync(WidgetDataSourceContext context)
		{
			if (this.ReportId.HasValue == false)
				throw new OrionsException($"No {nameof(ReportId)} assigned");

			if (this.MetadataSetId.HasValue == false)
				throw new OrionsException($"No {nameof(MetadataSetId)} assigned");

			var reportTemplate = await context.HyperStore.RetrieveAsync<HyperMetadataReport>(this.ReportId.Value);
			var metadataSet = await context.HyperStore.RetrieveAsync<HyperMetadataSet>(this.MetadataSetId.Value);

			var manager = new ReportExecutionManager(Logger.Instance);

			// Generate the basic template for the report.
			var templateData = await manager.GenerateTemplateAsync(context.HyperStore, reportTemplate, this.MetadataSetId.Value);

			if (context.DynamicFilter != null)
			{
				// Modify report template to fit runtime requirements.
				if (context.DynamicFilter is MultiFilterData multiFilter)
				{
					foreach (var textFilterData in multiFilter.Elements?.OfType<TextFilterData>() ?? Enumerable.Empty<TextFilterData>())
					{
						var columns = new List<ReportColumn>(templateData.ColumnsDefinitions);
						var rows = new List<ReportRow>(templateData.RowsDefinitions);

						foreach (var column in columns.ToArray())
						{
							if (textFilterData.Filter(new string[] { column.Title }, null) == false)
								columns.Remove(column);

							//foreach (var row in rows)
							//{
							//}
						}

						templateData.ColumnsDefinitions = columns.ToArray(); // Re-apply the filtered columns.
					}
				}

			}

			// Actually run the report.
			var data = await manager.ExecuteTemplate(context.HyperStore, reportTemplate, templateData, this.MetadataSetId.Value, false);

			var result = new HyperMetadataReportResult();
			result.Report = reportTemplate;
			result.Data = data;
			result.MetadataSet = metadataSet;

			return result;
		}

	}
}
