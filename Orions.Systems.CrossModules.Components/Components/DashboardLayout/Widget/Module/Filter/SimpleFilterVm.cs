using Microsoft.AspNetCore.Components.Web;
using Orions.Common;
using Orions.Infrastructure.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(SimpleFilterWidget))]
	public class SimpleFilterVm : WidgetVm<SimpleFilterWidget>
	{
		public PeriodDefinition[] AvailablePeriods { get; set; } = new PeriodDefinition[]
			{
				//new PeriodDefinition() { Period = TimePeriods.Minute, PeriodsCount = 15 },
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

		public DateTime MinDate
		{
			get
			{
				return this.Widget?.MinDate ?? new DateTime(2019, 1, 1);
			}
		}

		public DateTime MaxDate
		{
			get
			{
				return this.Widget?.MaxDate ?? DateTime.UtcNow.AddDays(15);
			}
		}

		public DateTime? StartDate
		{
			get
			{
				var filterGroup = this.DashboardVm?.ObtainFilterGroup(this.Widget.FilterGroup);
				return filterGroup?.StartTime;
			}

			set
			{
				var filterGroup = this.DashboardVm?.ObtainFilterGroup(this.Widget.FilterGroup);
				if (filterGroup != null)
					filterGroup.StartTime = value;
			}
		}

		public DateTime? EndDate
		{
			get
			{
				var filterGroup = this.DashboardVm?.ObtainFilterGroup(this.Widget.FilterGroup);
				return filterGroup?.EndTime;
			}

			set
			{
				var filterGroup = this.DashboardVm?.ObtainFilterGroup(this.Widget.FilterGroup);
				if (filterGroup != null)
					filterGroup.EndTime = value;
			}
		}

		public string[] Filters
		{
			get
			{
				//return Widget.Filters;

				var filterGroup = this.DashboardVm?.ObtainFilterGroup(this.Widget.FilterGroup);
				return filterGroup?.FilterLabels;
			}

			set
			{
				//Widget.Filters = value;

				var filterGroup = this.DashboardVm?.ObtainFilterGroup(this.Widget.FilterGroup);
				if (filterGroup != null)
				{
					filterGroup.FilterLabels = value;
				}
			}
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
			  new Option(){ ID= "Arms", Group = "Body parts", Text= "Arms" },
		 };

		public bool AllowDynamicFiltration
		{
			get
			{
				var filterGroup = this.DashboardVm?.ObtainFilterGroup(this.Widget.FilterGroup);
				if (filterGroup == null) return false;
				return filterGroup.AllowDynamicFiltration;
			}
			set
			{
				var filterGroup = this.DashboardVm?.ObtainFilterGroup(this.Widget.FilterGroup);
				if (filterGroup != null)
				{
					filterGroup.AllowDynamicFiltration = value;
				}
			}
		}

		public SimpleFilterVm()
		{
		}

		//public void OnChangeFilters(string[] filters) 
		//{
		//	this.RaiseNotify(nameof(Filters));
		//}

		protected override void OnWidgetSet(IDashboardWidget widget)
		{
			var filterGroup = this.DashboardVm?.ObtainFilterGroup(this.Widget.FilterGroup);

			if (filterGroup != null && this.Widget.PredefinedFilters?.Length > 0)
			{
				if (filterGroup.FilterLabels == null)
				{
					filterGroup.FilterLabels = this.Widget.PredefinedFilters;
				}
				else
				{
					filterGroup.FilterLabels = filterGroup.FilterLabels.Concat(this.Widget.PredefinedFilters).Distinct().ToArray();
				}
			}

			if (filterGroup != null) {
				filterGroup.AllowDynamicFiltration = this.Widget.AllowDynamicFiltration;
			}

			base.OnWidgetSet(widget);
		}

		private void FilterGroup_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(DashboardFilterGroupData.FilterLabels))
				this.RaiseNotify(nameof(Filters));

			if (e.PropertyName == nameof(DashboardFilterGroupData.StartTime))
				this.RaiseNotify(nameof(StartDate));

			if (e.PropertyName == nameof(DashboardFilterGroupData.EndTime))
				this.RaiseNotify(nameof(EndDate));
		}

		public void OnChangeView()
		{
			var filterGroup = this.DashboardVm.ObtainFilterGroup(this.Widget.FilterGroup);
			if (filterGroup == null)
				return;

			if (filterGroup.View == "test")
				filterGroup.View = "test2";
			else
				filterGroup.View = "test";
		}

		public void OnPeriodClick(PeriodDefinition period)
		{
			var filterGroup = this.DashboardVm.ObtainFilterGroup(this.Widget.FilterGroup);
			if (filterGroup == null)
				return;

			filterGroup.Period = period;
		}

		protected override void OnSetParentVm(BaseVm parentVm)
		{
			base.OnSetParentVm(parentVm);

			var group = this.DashboardVm.ObtainFilterGroup(this.Widget);

			if (group != null)
				group.PropertyChanged += FilterGroup_PropertyChanged;

			this.ApplyAsync(false).GetAwaiter().GetResult();
		}

		public async Task ClearAllFilters()
		{
			this.DashboardVm.ClearAllFilterGroups();
			await this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();
		}

		public async Task ClearFilters()
		{
			var filterGroup = this.DashboardVm.ObtainFilterGroup(this.Widget.FilterGroup);

			if (filterGroup == null) return;

			filterGroup.Clear();

			this.Widget.PredefinedFilters = null;

			await this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();
		}

		public async Task ApplyAsync(bool updateDashboard = true)
		{
			var filterGroup = this.DashboardVm.ObtainFilterGroup(this.Widget.FilterGroup);
			if (filterGroup == null)
				return;

			//if (this.Widget.Period != null)
			//	filterGroup.Period = this.Widget.Period;

			//filterGroup.FilterLabels = this.Widget.Filters;
			filterGroup.FilterTarget = this.Widget.FilterTarget;

			//filterGroup.StartTime = this.Widget.StartDate;
			//filterGroup.EndTime = this.Widget.EndDate;

			if (updateDashboard)
			{
				await this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();
				//await this.DashboardVm.SaveChangesAsync(); // Save the settings into the persistent storage.
			}
		}
	}
}
