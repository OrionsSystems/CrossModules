using System;
using System.Linq;
using System.Threading.Tasks;
using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(DashboardContainerWidget))]
	public class DashboardContainerVm : WidgetVm<DashboardContainerWidget>
	{		
		public DashboardContainerVm()
		{
		}

		public async Task<DashboardData> SetDashboardAsync()
		{
			if (string.IsNullOrWhiteSpace(this.Widget.Dashboard?.Id))
			{
				return null;
			}

			var dashboards = await HyperStore.FindAllAsync<DashboardData>();

			var dashboard = dashboards.FirstOrDefault(it => it.Id == this.Widget.Dashboard?.Id);

			if (dashboard == null)
				return null;

			(this.ParentVm as DashboardVm).Source = dashboard;

			return dashboard;
		}
	}
}
