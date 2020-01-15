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

		protected override void OnSetParentVm(BaseVm parentVm)
		{
			base.OnSetParentVm(parentVm);

			if (this.DashboardVm != null && this.Widget.Filters?.Length > 0)
				this.DashboardVm.SetStringFilters(this.Widget.Filters, this.Widget.FilterTarget);
		}

		public async Task ApplyAsync(string[] filters)
		{
			this.Widget.Filters = filters;

			this.DashboardVm.SetStringFilters(filters, this.Widget.FilterTarget);

			await this.DashboardVm.SaveChangesAsync(); // Save the settings into the persistent storage.

			await this.DashboardVm.UpdateDynamicWidgetsAsync();
		}
	}
}
