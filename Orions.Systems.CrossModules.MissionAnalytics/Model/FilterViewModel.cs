using System;
using System.Collections.Generic;
using System.Linq;

namespace Orions.Systems.CrossModules.MissionAnalytics.Model
{
	public class FilterViewModel
	{
		public IEnumerable<MissionInstanceItemViewModel> MissionInstances { get; set; }
		public string SelectedMissionInstance { get; set; }

		public IEnumerable<SelectListItem> StagesOptions { get; set; }
		public string SelectedStage { get; set; }

		public IEnumerable<SelectListItem> DaysOptions { get; set; }

		public string SelectedDayOption
		{
			get
			{
				var selected = DaysOptions.FirstOrDefault(it => it.Selected);
				return selected?.Value;
			}
			set
			{
				foreach (var option in DaysOptions)
				{
					option.Selected = false;
				}

				var selected = DaysOptions.FirstOrDefault(it => it.Value == value);
				if (selected == null) return;
				selected.Selected = true;
			}
		}

		public string TemplateId { get; set; } = "3";

		public double ReportDaysValue
		{
			get
			{
				double.TryParse(SelectedDayOption, out var days);
				return days;
			}
		}

		public int TimeStep
		{
			get
			{
				var value = Math.Round(ReportDaysValue, 4);
				switch (value)
				{
					case (0.0417): // Last Hour
						return 2;
					case (0.0833): // Last 2 Hours
						return 5;
					case (0.125): // Last 3 Hours
						return 10;
					case (0.25): // Last 6 Hours
						return 20;
					case (0.5): // Last 12 Hours
						return 25;
					case (1): // Last Day
						return 30;
					case (3): // Last 3 days
						return 60;
					case (7): // Last Week
						return 120;
					case (30): // Last Month
						return 360; // 6 hours
					case (180): // Last 6 Months
						return 4320; // 3 days
					case (365): // Last Year
						return 10080; // 1 week
					case (0): // All Time
						return 1440 * 30; // 1 month
					default:
						return 5;
				}
			}
		}

		public FilterViewModel()
		{
			DaysOptions = SetDaysOptions();
			StagesOptions = SetStagesOptions();
		}

		private IEnumerable<SelectListItem> SetDaysOptions()
		{
			return new List<SelectListItem>
			{
				new SelectListItem { Text = "Last Hour", Value = "0.0417"},
				new SelectListItem { Text = "Last 2 Hours", Value = "0.0833"},
				new SelectListItem { Text = "Last 3 Hours", Value = "0.125"},
				new SelectListItem { Text = "Last 6 Hours", Value = "0.25"},
				new SelectListItem { Text = "Last 12 Hours", Value = "0.5"},
				new SelectListItem { Text = "Last Day", Value = "1"},
				new SelectListItem { Text = "Last 3 days", Value = "3"},
				new SelectListItem { Text = "Last Week", Value = "7", Selected = true },
				new SelectListItem { Text = "Last Month", Value = "30"},
				new SelectListItem { Text = "Last 6 Months", Value = "180"},
				new SelectListItem { Text = "Last Year", Value = "365"},
				new SelectListItem { Text = "All Time", Value = "0" }
			}.AsQueryable();
		}

		private IEnumerable<SelectListItem> SetStagesOptions()
		{
			return new List<SelectListItem>
			{
				new SelectListItem { Text = "All Mission Stages", Value = "0", Selected = true },
				new SelectListItem { Text = "Mission Active Stages", Value = "1" }
			}.AsQueryable();
		}
	}
}
