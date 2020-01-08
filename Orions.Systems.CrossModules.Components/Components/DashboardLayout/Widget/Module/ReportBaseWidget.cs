using Orions.Common;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class WidgetDataSource : UnifiedBlob
	{
		public WidgetDataSource()
		{
		}
	}

	public class CSVWidgetDataSource : WidgetDataSource
	{
		[UniBrowsable(UniBrowsableAttribute.EditTypes.TextFile)]
		[HelpText("Add report result document", HelpTextAttribute.Priorities.Important)]
		public byte[] Data { get; set; }

		public CSVWidgetDataSource()
		{
		}
	}

	public class ReportResultWidgetDataSource : WidgetDataSource
	{
		[HelpText("Add report result document", HelpTextAttribute.Priorities.Important)]
		[HyperDocumentId.DocumentType(typeof(HyperMetadataReportResult))]
		public HyperDocumentId ReportResultId { get; set; }

		public ReportResultWidgetDataSource()
		{
		}
	}

	public abstract class ReportBaseWidget : DashboardWidgetBase, IDashboardWidget
	{
		[HelpText("Add the data for this widget to use")]
		public WidgetDataSource DataSource { get; set; } = new ReportResultWidgetDataSource();

		public ReportBaseWidget()
		{
		}
	}
}
