using Microsoft.AspNetCore.Components.Web;
using Orions.Common;
using Orions.Infrastructure.Reporting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(SimpleFilterWidget))]
	public class SimpleFilterVm : WidgetVm<SimpleFilterWidget>
	{
		public PeriodDefinition[] AvailablePeriods { get; set; } = new PeriodDefinition[]
			{
				new PeriodDefinition() { Period = TimePeriods.Minute, PeriodsCount = 15 },
				new PeriodDefinition() { Period = TimePeriods.Hour, PeriodsCount = 1 },
				new PeriodDefinition() { Period = TimePeriods.Hour, PeriodsCount = 4 },
				new PeriodDefinition() { Period = TimePeriods.Hour, PeriodsCount = 12 },
				new PeriodDefinition() { Period = TimePeriods.Day, PeriodsCount = 1 },
			};

		public class Option
		{
			public string ID { get; set; }
			public string Group { get; set; }
			public string Text { get; set; }
		}

		public Option[] Options = new Option[]
		{
			  new Option(){ ID= "Person", Group = "Common", Text= "Person" },
			  new Option(){ ID= "Human", Group = "Common", Text= "Human" },
			  new Option(){ ID= "Adult", Group = "Common", Text= "Adult" },
			  new Option(){ ID= "Male", Group = "Common", Text= "Male" },
			  new Option(){ ID= "Female", Group = "Common", Text= "Female" },
			  new Option(){ ID= "Child", Group = "Common", Text= "Child" },
			  new Option(){ ID= "Father", Group = "Common", Text= "Father" },
			  new Option(){ ID= "Mother", Group = "Common", Text= "Mother" },
			  new Option(){ ID= "Son", Group = "Common", Text= "Son" },
			  new Option(){ ID= "Daughter", Group = "Common", Text= "Daughter" },
			  new Option(){ ID= "Car", Group = "Common", Text= "Car" },
			  new Option(){ ID= "Head", Group = "Body parts", Text= "Head" },
		 };

		public SimpleFilterVm()
		{
		}

		public void OnPeriodClick(PeriodDefinition period)
		{
			var filterGroup = this.DashboardVm.ObtainFilterData(this.Widget.FilterGroup);
			if (filterGroup == null)
				return;

			filterGroup.Period = period;
		}

		protected override void OnSetParentVm(BaseVm parentVm)
		{
			base.OnSetParentVm(parentVm);
			this.ApplyAsync(false).GetAwaiter().GetResult();
		}

		public async Task ClearAllFilters()
		{
			this.DashboardVm.ClearAllFilterGroups();
			await this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();
		}

		public async Task ClearFilters()
		{
			this.DashboardVm.ClearFilterGroup(this.Widget.FilterGroup);
			await this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();
		}

		public async Task ApplyAsync(bool updateDashboard = true)
		{
			var filterGroup = this.DashboardVm.ObtainFilterData(this.Widget.FilterGroup);
			if (filterGroup == null)
				return;

			if (this.Widget.Period != null)
				filterGroup.Period = this.Widget.Period;

			filterGroup.FilterLabels = this.Widget.Filters;
			filterGroup.FilterTarget = this.Widget.FilterTarget;
			
			filterGroup.StartTime = this.Widget.StartDate;
			filterGroup.EndTime = this.Widget.EndDate;

			if (updateDashboard)
			{
				await this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();
				await this.DashboardVm.SaveChangesAsync(); // Save the settings into the persistent storage.
			}
		}
	}
}
