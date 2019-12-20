using Orions.Infrastructure.Reporting;
using Orions.Node.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportBaseWidget : DashboardWidgetBase, IDashboardWidget
	{
		[HyperDocumentId.DocumentType(typeof(HyperMetadataReportResult))]
		public HyperDocumentId ReportResultId { get; set; }

		public override Type GetViewComponent()
		{
			return typeof(ReportGrid);
		}
	}
}
