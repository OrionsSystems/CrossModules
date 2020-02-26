using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportBaseWidget : DashboardWidget
	{
		[HelpText("Add the data for this widget to use", HelpTextAttribute.Priorities.Important)]
		public WidgetDataSource DataSource { get; set; } = new ReportModelWidgetDataSource();

		public ReportBaseWidget()
		{
		}
	}
}
