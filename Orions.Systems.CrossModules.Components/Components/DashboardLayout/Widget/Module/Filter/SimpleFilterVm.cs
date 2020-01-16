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
				this.DashboardVm.SetStringFilters(this.Widget.FilterGroup, this.Widget.Filters, this.Widget.FilterTarget);

			if (this.DashboardVm != null && this.Widget.StartDate.HasValue && this.Widget.EndDate.HasValue)
				this.DashboardVm.SetDateTimeFilters(this.Widget.FilterGroup, this.Widget.StartDate, this.Widget.EndDate, this.Widget.FilterTarget);

			//await this.DashboardVm.UpdateDynamicWidgetsAsync();
		}

		public async Task ApplyAsync(string[] filters, DateTime? startTime, DateTime? endTime)
		{
			this.Widget.Filters = filters;
			this.Widget.StartDate = startTime;
			this.Widget.EndDate = endTime;

			this.DashboardVm.SetStringFilters(this.Widget.FilterGroup, filters, this.Widget.FilterTarget);

			this.DashboardVm.SetDateTimeFilters(this.Widget.FilterGroup, this.Widget.StartDate, this.Widget.EndDate, this.Widget.FilterTarget);

			await this.DashboardVm.SaveChangesAsync(); // Save the settings into the persistent storage.

			await this.DashboardVm.UpdateDynamicWidgetsAsync();
		}
	}
}
