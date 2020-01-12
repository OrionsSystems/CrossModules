using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Text;
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

			var manager = new ReportExecutionManager(Logger.Instance);

			if (context.DynamicFilter != null)
			{
				// Run a template mode call to generate the base data we want to apply dynamic filtering to.
				var templateData = manager.ExecuteAsync(context.HyperStore, reportTemplate, this.MetadataSetId.Value, false, null, true);

				// Modify report template to fit runtime requirements.
				if (context.DynamicFilter is TextFilterData textFilterData)
				{

				}
			}

			var data = await manager.ExecuteAsync(context.HyperStore, reportTemplate, this.MetadataSetId.Value, false);

			var result = new HyperMetadataReportResult();

			return result;
		}

	}
}
