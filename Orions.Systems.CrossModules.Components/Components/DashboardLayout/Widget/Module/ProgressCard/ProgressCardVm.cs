using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(ProgressCardWidget))]
	public class ProgressCardVm : ReportWidgetVm<ProgressCardWidget>
	{
		public ProgressCardVm()
		{
		}
	}
}
