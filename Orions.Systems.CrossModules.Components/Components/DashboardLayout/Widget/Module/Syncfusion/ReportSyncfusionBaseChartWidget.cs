using Syncfusion.EJ2.Blazor;
using Syncfusion.EJ2.Blazor.Charts;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportSyncfusionBaseChartWidget : ReportBaseWidget, IDashboardWidget
	{
		public bool IsShowChartTitle { get; set; } = false;
		public string ChartTitle { get; set; }

	
	}
}
