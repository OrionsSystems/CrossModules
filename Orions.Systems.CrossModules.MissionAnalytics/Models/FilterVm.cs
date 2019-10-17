using System;
using System.Collections.Generic;
using System.Linq;
using Orions.Systems.CrossModules.Blazor;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class FilterVm : BlazorVm
	{
		public List<SelectListItem> MissionInstanceOptions { get; set; }
		
		public string SelectedMissionInstance
		{
			get => GetSelectedValue(MissionInstanceOptions);
			set => SetSelectedValue(MissionInstanceOptions, value);
		}

		public List<SelectListItem> TimeRangeOptions{ get; set; }

		public string SelectedTimeRange
		{
			get => GetSelectedValue(TimeRangeOptions);
			set => SetSelectedValue(TimeRangeOptions, value);
		}

		public FilterVm()
		{
			MissionInstanceOptions= new List<SelectListItem>();
			TimeRangeOptions = new List<SelectListItem>();
		}

		private static string GetSelectedValue(
			IEnumerable<SelectListItem> items)
		{
			return items?.FirstOrDefault(it => it.Selected)?.Value ?? "";
		}

		private static void SetSelectedValue(
			IEnumerable<SelectListItem> items, 
			string value)
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
