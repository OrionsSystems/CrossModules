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
	public class ReportModelWidgetDataSource : ActiveFilterWidgetDataSource
	{
		[HelpText("The Report to use as a template for this source", HelpTextAttribute.Priorities.Mandatory)]
		[HyperDocumentId.DocumentType(typeof(ReportModelConfig), RetrievePayload = false)]
		public HyperDocumentId? ReportModelId { get; set; }

		[HelpText("Execute the report as a job on the server; recommended to use, unless specific reasons against, as it is MUCH faster")]
		public bool JobMode { get; set; } = true;

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

			if (context.GroupFilterData?.FilterLabels?.Length > 0)
				reportModelConfig.TrySetLabels(context.GroupFilterData?.FilterLabels, context.GroupFilterData.FilterTarget);

			var helper = new ReportModelHelper();
			if (this.JobMode)
			{
				return await helper.ExecuteJobModeAsync(context.HyperStore, reportModelConfig, null, context.CancellationToken, "");
			}
			else
			{
				return await helper.ExecuteLocalModeAsync(context.HyperStore, reportModelConfig, null, "", context.Logger, context.CancellationToken);
			}
		}

	}
}
