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
	public class ReportResultWidgetDataSource : WidgetDataSource
	{
		[HelpText("Add report result document", HelpTextAttribute.Priorities.Important)]
		[HyperDocumentId.DocumentType(typeof(HyperMetadataReportResult))]
		public HyperDocumentId ReportResultId { get; set; }

		public ReportResultWidgetDataSource()
		{
		}

		public override async Task<IReportResult> GenerateReportResultAsync(WidgetDataSourceContext context)
		{
			var args = new RetrieveHyperDocumentArgs(this.ReportResultId);
			var doc = await context.HyperStore.ExecuteAsync(args);

			if (args.ExecutionResult.IsNotSuccess)
			{
				return null;
			}

			return doc?.GetPayload<HyperMetadataReportResult>();
		}

	}
}
