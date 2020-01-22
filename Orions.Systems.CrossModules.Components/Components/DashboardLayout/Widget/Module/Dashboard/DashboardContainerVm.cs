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

		public async Task<DashboardData> SetSelectedDashboard()
		{
			if (string.IsNullOrWhiteSpace(this.Widget.DashboardElementName))
			{
				return null;
			}

			var datas = await HyperStore.FindAllAsync<DashboardData>();

			var data = datas.FirstOrDefault(it => it.Name == this.Widget.DashboardElementName);

			if (data == null)
				return null;

			(this.ParentVm as DashboardVm).Source = data;

			return data;
		}
	}
}
