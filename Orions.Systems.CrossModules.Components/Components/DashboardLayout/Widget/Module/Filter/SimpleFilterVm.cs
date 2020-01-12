using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Syncfusion.EJ2.Blazor.DropDowns;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(SimpleFilterWidget))]
	public class SimpleFilterVm : WidgetVm<SimpleFilterWidget>
	{
		public SimpleFilterVm()
		{
		}

		public async Task ApplyAsync(string[] filters)
		{
			this.DashboardVm.SetStringFilters(filters);
			await this.DashboardVm.UpdateDynamicWidgetsAsync();
		}
	}
}
