using Syncfusion.EJ2.Blazor;
using Syncfusion.EJ2.Blazor.Charts;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportSyncfusionBaseAccumulationChartWidget : ReportChartWidget, IDashboardWidget
	{

		public class AccumulationDataLabelSettings
		{
			public bool Visible { get; set; } = true;
			public AccumulationLabelPosition Position { get; set; } = AccumulationLabelPosition.Outside;
		}

		public string Height { get; set; }

		public string Width { get; set; }

		public bool IsShowChartTitle { get; set; } = false;

		public string ChartTitle { get; set; }

		public SyncfiusionLegendDefinition LegendSettings { get; set; } = new SyncfiusionLegendDefinition();

		public AccumulationDataLabelSettings LabelSettings { get; set; } = new AccumulationDataLabelSettings();

		public AccEmptyPointMode PointMode { get; set; } = AccEmptyPointMode.Zero;
		public AccumulationType AccumulationType { get; set; } = AccumulationType.Pie;

		public bool IsEnableTooltip { get; set; } = true;

		public string Radius { get; set; } = "100%";

		public string InnerRadius { get; set; } = "0%";

		public ReportSyncfusionBaseAccumulationChartWidget()
		{
		}
	}
}
