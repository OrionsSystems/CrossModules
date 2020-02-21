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
	[Compatibility("DynamicReportResultWidgetDataSource")]
	public class ReportModelWidgetDataSource : WidgetDataSource
	{
		[HelpText("The Report to use as a template for this source", HelpTextAttribute.Priorities.Mandatory)]
		[HyperDocumentId.DocumentType(typeof(ReportModelConfig))]
		public HyperDocumentId? ReportModelId { get; set; }

		//[HelpText("The Metadata set to use as a source", HelpTextAttribute.Priorities.Mandatory)]
		//[HyperDocumentId.DocumentType(typeof(HyperMetadataSet))]
		//public HyperDocumentId? MetadataSetId { get; set; }

		public override bool SupportsDynamicFiltration => true;

		public ReportModelWidgetDataSource()
		{
		}

		protected override async Task<Report> OnGenerateFilteredReportResultAsync(WidgetDataSourceContext context)
		{
			if (this.ReportModelId.HasValue == false)
				throw new OrionsException($"No {nameof(ReportModelId)} assigned");

			var reportModelConfig = await context.HyperStore.RetrieveAsync<ReportModelConfig>(this.ReportModelId.Value);

			if (context.GroupFilterData?.StartTime.HasValue == true || context.GroupFilterData?.EndTime.HasValue == true)
				reportModelConfig.TrySetTimeRange(context.GroupFilterData?.StartTime, context.GroupFilterData?.EndTime);

			if (context.GroupFilterData?.Period != null)
				reportModelConfig.TrySetPeriod(context.GroupFilterData.Period);

			var startJobArgs = new StartHyperJobArgs(typeof(ReportModelJob), 
				new ReportModelJobConfig() { ModelConfig = reportModelConfig }) { WaitForResult = true };

			var result = await context.HyperStore.ExecuteAsync(startJobArgs);

			var executionResult = (ExecutionResult<Report>)result.JobResult;

			return executionResult.Value;
		}

	}
}
