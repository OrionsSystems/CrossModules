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
		[HyperDocumentId.DocumentType(typeof(MetadataSetReport))]
		public HyperDocumentId? ReportId { get; set; }

		[HelpText("The Metadata set to use as a source", HelpTextAttribute.Priorities.Mandatory)]
		[HyperDocumentId.DocumentType(typeof(HyperMetadataSet))]
		public HyperDocumentId? MetadataSetId { get; set; }

		public override bool SupportsDynamicFiltration => true;

		public DynamicReportResultWidgetDataSource()
		{
		}

		protected override async Task<Report> OnGenerateFilteredReportResultAsync(WidgetDataSourceContext context)
		{
			if (this.ReportId.HasValue == false)
				throw new OrionsException($"No {nameof(ReportId)} assigned");

			if (this.MetadataSetId.HasValue == false)
				throw new OrionsException($"No {nameof(MetadataSetId)} assigned");

			var reportTemplate = await context.HyperStore.RetrieveAsync<MetadataSetReport>(this.ReportId.Value);
			var metadataSet = await context.HyperStore.RetrieveAsync<HyperMetadataSet>(this.MetadataSetId.Value);

			throw new NotImplementedException();

			//var manager = new ReportExecutionManager(Logger.Instance);

			//// Generate the basic template for the report.
			//var templateData = await manager.GenerateTemplateAsync(context.HyperStore, reportTemplate, this.MetadataSetId.Value, context.DynamicFilter);

			//if (context.DynamicFilter != null)
			//	templateData.FilterWith(context.DynamicFilter);

			//// Actually run the report.
			//var data = await manager.ExecuteTemplate(context.HyperStore, reportTemplate, templateData, this.MetadataSetId.Value, false);

			//var result = new HyperMetadataReportResult();
			//result.Report = reportTemplate;
			//result.Data = data;
			//result.MetadataSet = metadataSet;

			//return result;
		}

	}
}
