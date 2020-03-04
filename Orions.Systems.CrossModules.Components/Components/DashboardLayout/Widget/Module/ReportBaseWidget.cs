using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportBaseWidget : DashboardWidget, IReportDashboardWidget
	{
		[HelpText("Add the data for this widget to use", HelpTextAttribute.Priorities.Important)]
		public WidgetDataSource DataSource { get; set; } = new ReportModelWidgetDataSource();

		[HelpText("If enabled a % will be added at the end of each value displayed")]
		public bool AppendPercentageSign { get; set; }

		public ReportBaseWidget()
		{
		}
	}
}
