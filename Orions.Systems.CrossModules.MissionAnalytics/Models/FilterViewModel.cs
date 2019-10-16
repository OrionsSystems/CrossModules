using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class FilterViewModel
	{
		public IEnumerable<SelectListItem> MissionInstanceOptions { get; set; }
		
		public string SelectedMissionInstance 
		{
			get => GetSelectedValue(MissionInstanceOptions);
			set => SetSelectedValue(MissionInstanceOptions, value);
		}

		public IEnumerable<SelectListItem> StageOptions { get; set; }

		public string SelectedStage
		{
			get => GetSelectedValue(StageOptions);
			set => SetSelectedValue(StageOptions, value);
		}

		public IEnumerable<SelectListItem> TimeRangeOptions { get; set; }

		public string SelectedTimeRange
		{
			get => GetSelectedValue(TimeRangeOptions);
			set => SetSelectedValue(TimeRangeOptions, value);
		}

		public double TimeRangeValue
		{
			get
			{
				double.TryParse(SelectedTimeRange, out var days);
				return Math.Round(days, 4);
			}
		}

		public int TimeStep
		{
			get
			{
				switch (TimeRangeValue)
				{
					case Model.TimeRangeOptions.LastHour:
						//return 2;
						return 10;
					case Model.TimeRangeOptions.Last2Hours:
						//return 5;
						return 10;
					case Model.TimeRangeOptions.Last3Hours:
						//return 10;
						return 15;
					case Model.TimeRangeOptions.Last6Hours:
						//return 20;
						return 30;
					case Model.TimeRangeOptions.Last12Hours:
						//return 25;
						return 60;
					case Model.TimeRangeOptions.LastDay:
						//return 30;
						return 2 * 60;
					case Model.TimeRangeOptions.Last3Days:
						//return 60;
						return 6 * 60;
					case Model.TimeRangeOptions.LastWeek:
						// return 2 * 60
						return 24 * 60;
					case Model.TimeRangeOptions.LastMonth:
						//return 6 * 60; 
						return 3 * 24 * 60;
					case Model.TimeRangeOptions.Last3Months:
						//return 3 * 24 * 60; 
						return 10 * 24 * 60;
					case Model.TimeRangeOptions.Last6Months:
						//return 3 * 24 * 60; 
						return 15 * 24 * 60;
					case Model.TimeRangeOptions.LastYear:
						//return 7 * 24 * 60; 
						return 30 * 24 * 60;
					case Model.TimeRangeOptions.Ever:
						//return 30 * 24 * 60;
						return 30 * 24 * 60;
					default:
						throw new NotImplementedException();
				}
			}
		}

		public string DateTimeFormatString
		{
			get
			{
				switch (TimeRangeValue)
				{
					case Model.TimeRangeOptions.LastHour:
						return "h:mm tt";
					case Model.TimeRangeOptions.Last2Hours:
						return "h:mm tt";
					case Model.TimeRangeOptions.Last3Hours:
						return "h:mm tt";
					case Model.TimeRangeOptions.Last6Hours:
						return "h:mm tt";
					case Model.TimeRangeOptions.Last12Hours:
						return "h:mm tt";
					case Model.TimeRangeOptions.LastDay:
						return "M/d h tt";
					case Model.TimeRangeOptions.Last3Days:
						return "M/d h tt";
					case Model.TimeRangeOptions.LastWeek:
						return "d";
					case Model.TimeRangeOptions.LastMonth:
						return "d";
					case Model.TimeRangeOptions.Last3Months:
						return "d";
					case Model.TimeRangeOptions.Last6Months:
						return "d";
					case Model.TimeRangeOptions.LastYear:
						return "M/yyyy";
					case Model.TimeRangeOptions.Ever:
						return "M/yyyy";
					default:
						throw new NotImplementedException();
				}
			}
		}

		public FilterViewModel()
		{
			TimeRangeOptions = GetTimeRangeOptions();
			StageOptions = GetStageOptions();
		}

		private static IEnumerable<SelectListItem> GetTimeRangeOptions()
		{
			return new List<SelectListItem>
			{
				new SelectListItem { Text = "Last Hour", Value = Model.TimeRangeOptions.LastHour.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 2 Hours", Value = Model.TimeRangeOptions.Last2Hours.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 3 Hours", Value = Model.TimeRangeOptions.Last3Hours.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 6 Hours", Value = Model.TimeRangeOptions.Last6Hours.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 12 Hours", Value = Model.TimeRangeOptions.Last12Hours.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last Day", Value = Model.TimeRangeOptions.LastDay.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 3 days", Value = Model.TimeRangeOptions.Last3Days.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last Week", Value = Model.TimeRangeOptions.LastWeek.ToString(CultureInfo.InvariantCulture), Selected = true },
				new SelectListItem { Text = "Last Month", Value = Model.TimeRangeOptions.LastMonth.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 3 Months", Value = Model.TimeRangeOptions.Last3Months.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 6 Months", Value = Model.TimeRangeOptions.Last6Months.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last Year", Value = Model.TimeRangeOptions.LastYear.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "All Time", Value = Model.TimeRangeOptions.Ever.ToString(CultureInfo.InvariantCulture) }
			}.AsQueryable();
		}

		private static IEnumerable<SelectListItem> GetStageOptions()
		{
			return new List<SelectListItem>
			{
				new SelectListItem { Text = "All Stages", Value = "0", Selected = true },
				new SelectListItem { Text = "Active Stages", Value = "1" }
			}.AsQueryable();
		}

		private static string GetSelectedValue(IEnumerable<SelectListItem> items)
		{
			return items?.FirstOrDefault(it => it.Selected)?.Value;
		}

		private static void SetSelectedValue(IEnumerable<SelectListItem> items, string value)
		{
			if (items == null) throw new ArgumentException(nameof(items));

			foreach (var option in items)
			{
				option.Selected = false;
			}

			var selected = items.FirstOrDefault(it => it.Value == value);
			if (selected == null) return;
			selected.Selected = true;
		}
	}
}
