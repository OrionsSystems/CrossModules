using System;
using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Timeline.Models
{
	public class FilterViewModel
	{
		public string PageSize { get; set; }
		public string Input { get; set; }
		public string TimeFilterValue { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public int TimeOutInSeconds { get; set; } = 10;

		public ulong? FromSeconds { get; set; }
		public ulong? ToSeconds { get; set; }

		public int PageSizeValue
		{
			get
			{
				var res = 0;
				int.TryParse(PageSize, out res);
				return res;
			}
		}

		public int PageNumber { get; set; }

		public IEnumerable<SelectListItem> PageSizeOptions { get; set; }

		public IEnumerable<SelectListItem> TimeFilterOptions { get; set; }

		public FilterViewModel()
		{
			PageSizeOptions = GetPageSizeOptionsOptions();
			TimeFilterOptions = GetTimeFilterOptions();
		}

		private IEnumerable<SelectListItem> GetPageSizeOptionsOptions()
		{
			return new List<SelectListItem> {
				new SelectListItem(){ Text="20", Value="20", Selected = PageSizeValue == 20  },
				new SelectListItem(){ Text="50", Value="50", Selected = PageSizeValue == 50 },
				new SelectListItem(){ Text="100", Value="100", Selected = PageSizeValue == 100 || PageSizeValue == 0},
				new SelectListItem(){ Text="500", Value="500", Selected = PageSizeValue == 500 },
				new SelectListItem(){ Text="1000", Value="1000", Selected = PageSizeValue == 1000 }
			};
		}

		private IEnumerable<SelectListItem> GetTimeFilterOptions()
		{
			return new List<SelectListItem> {
				new SelectListItem(){ Text="Wall Clock Time", Value="WallClockTime", Selected = TimeFilterValue == "WallClockTime" },
				new SelectListItem(){ Text="Stream Position", Value="StreamPosition", Selected = TimeFilterValue == "StreamPosition" }
			};
		}
	}
}
