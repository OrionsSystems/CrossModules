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

	

		public FilterViewModel()
		{
			TimeRangeOptions = GetTimeRangeOptions();
			StageOptions = GetStageOptions();
		}

		private static IEnumerable<SelectListItem> GetTimeRangeOptions()
		{
			return new List<SelectListItem>
			{
				new SelectListItem { Text = "Last Hour", Value = TimeRange.LastHour.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 2 Hours", Value = TimeRange.Last2Hours.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 3 Hours", Value = TimeRange.Last3Hours.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 6 Hours", Value = TimeRange.Last6Hours.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 12 Hours", Value = TimeRange.Last12Hours.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last Day", Value = TimeRange.LastDay.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 3 days", Value = TimeRange.Last3Days.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last Week", Value = TimeRange.LastWeek.ToString(CultureInfo.InvariantCulture), Selected = true },
				new SelectListItem { Text = "Last Month", Value = TimeRange.LastMonth.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 3 Months", Value = TimeRange.Last3Months.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last 6 Months", Value = TimeRange.Last6Months.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "Last Year", Value = TimeRange.LastYear.ToString(CultureInfo.InvariantCulture)},
				new SelectListItem { Text = "All Time", Value = TimeRange.Ever.ToString(CultureInfo.InvariantCulture) }
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
