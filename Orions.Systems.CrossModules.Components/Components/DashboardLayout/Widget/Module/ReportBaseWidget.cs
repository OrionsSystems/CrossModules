using Orions.Common;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportBaseWidget : DashboardWidgetBase, IDashboardWidget
	{
		[HelpText("Add report result document", HelpTextAttribute.Priorities.Important)]
		[HyperDocumentId.DocumentType(typeof(HyperMetadataReportResult))]
		public HyperDocumentId ReportResultId { get; set; }
	}
}
