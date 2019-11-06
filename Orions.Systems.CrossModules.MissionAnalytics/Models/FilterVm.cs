using System;
using System.Collections.Generic;
using System.Linq;
using Orions.Systems.CrossModules.Components;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class FilterVm : BlazorVm
	{
		public List<Option> MissionInstanceOptions { get; set; }
		
		public string SelectedMissionInstance
		{
			get => GetSelectedValue(MissionInstanceOptions);
			set => SetSelectedValue(MissionInstanceOptions, value);
		}

		public List<Option> TimeRangeOptions{ get; set; }

		public string SelectedTimeRange
		{
			get => GetSelectedValue(TimeRangeOptions);
			set => SetSelectedValue(TimeRangeOptions, value);
		}

		public FilterVm()
		{
			MissionInstanceOptions= new List<Option>();
			TimeRangeOptions = new List<Option>();
		}

		private static string GetSelectedValue(
			IEnumerable<Option> items)
		{
			return items?.FirstOrDefault(it => it.Selected)?.Value ?? "";
		}

		private static void SetSelectedValue(
			IEnumerable<Option> items, 
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
