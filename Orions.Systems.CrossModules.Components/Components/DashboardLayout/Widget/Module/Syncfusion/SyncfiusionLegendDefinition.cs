using Syncfusion.EJ2.Blazor.Charts;

namespace Orions.Systems.CrossModules.Components
{
	public class SyncfiusionLegendDefinition
	{
		public bool Visible { get; set; } = true;
		public LegendPosition Position { get; set; } = LegendPosition.Top;
		public Alignment Alignment { get; set; } = Alignment.Far;

	}
}
