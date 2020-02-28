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

		[HelpText("Metadata Set which will be used as source", HelpTextAttribute.Priorities.Mandatory)]
		[HyperDocumentId.DocumentType(typeof(MetadataSet), RetrievePayload = false)]
		public HyperDocumentId? MetadataSetId { get; set; }

		[HelpText("Execute the report as a job on the server; recommended to use, unless specific reasons against, as it is MUCH faster")]
		public bool JobMode { get; set; } = true;

		[HelpText("Allow Job mode to use cached reports (faster)")]
		public bool JobMode_CacheRead { get; set; } = true;

		[HelpText("Allow Job mode to store cached results (faster on subsequent calls)")]
		public bool JobMode_CacheWrite { get; set; } = true;

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

			if (MetadataSetId.HasValue)
			{
				var metadataSetStage = reportModelConfig.DataSource as MetadataSetReportDataSourceStageConfig;
				if (metadataSetStage != null)
					metadataSetStage.MetadataSetId = MetadataSetId;
				else
					Logger.Instance?.Warning(this, nameof(OnGenerateFilteredReportResultAsync), "Failed to apply MetadataSetId, since source stage is not a metadataset stage");
			}

			var helper = new ReportModelHelper();
			if (this.JobMode)
			{
				return await helper.ExecuteJobModeAsync(context.HyperStore, reportModelConfig, null, context.CancellationToken, "", this.JobMode_CacheRead, this.JobMode_CacheWrite);
			}
			else
			{
				return await helper.ExecuteLocalModeAsync(context.HyperStore, reportModelConfig, null, "", context.Logger, context.CancellationToken);
			}
		}

	}
}
