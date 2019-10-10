using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orions.Systems.CrossModules.MissionAnalytics.Pages
{
	public class IndexModel : PageModel
	{
		public IndexModel()
		{
		}

		[BindProperty(SupportsGet = true)]
		public string Instance { get; set; }

		[BindProperty(SupportsGet = true)]
		public string Stage { get; set; }

		[BindProperty(SupportsGet = true)]
		public string NodeId { get; set; }

		[BindProperty(SupportsGet = true)]
		public string[] WorkflowInstanceIds { get; set; }

		[BindProperty(SupportsGet = true)]
		public string ReportDays { get; set; } = "7";

		[BindProperty(SupportsGet = true)]
		public string MissionId { get; set; }

		public double ReportDaysValue {
			get {
				double days = 0;
				Double.TryParse(ReportDays, out days);
				return days;
			}
		}

		public int TimeStep
		{
			get
			{
				switch (Math.Round(ReportDaysValue, 3)) {
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
						return 360; //6 hours
					case (92): // Last 3 Months
						return 1440; //1 day
					case (180): // Last 6 Months
						return 4320; //3 days
					case (365): // Last Year
						return 10080; //a week
					case (0): // All Time
						return 1440 * 30; //a month
					default:
						return 5;
				}
			}
		}

		[BindProperty(SupportsGet = true)]
		public string TemplateId { get; set; } = "3";

		public List<SelectListItem> DaysOptions { get; set; }
		public List<SelectListItem> StagesOptions { get; set; }

		public void OnGetAsync()
		{
			var z = Request;
			if (z.Query.Any(it => it.Key == "request"))
			{
				var instructionQuery = z.Query.FirstOrDefault(it => it.Key == "request");

				string json = Uri.UnescapeDataString(instructionQuery.Value);
				var request = new CrossModuleVisualizationRequest();
				JsonHelper.Populate(json, request);

				var rMissionId = request.MissionIds?.First();
				if (!string.IsNullOrEmpty(rMissionId))
				{
					var mId = HyperDocumentId.TryParse(rMissionId);
					if (mId != null) MissionId = mId.Value.Id;
				}

				var wInstIds = request.WorkflowInstanceIds;
				if (wInstIds != null && wInstIds.Any())
				{
					var res = new List<string>();
					foreach (var wInstanceId in wInstIds) {
						if (!string.IsNullOrEmpty(wInstanceId))
						{
							var wid = HyperDocumentId.TryParse(wInstanceId);
							if (wid != null)
							{
								res.Add(wid.Value.Id);
							}
						}
					}
					WorkflowInstanceIds = res.ToArray();
				}
			}

			// Debug info
			//WorkflowInstanceIds = new[] { "c99da571" };
			//MissionId = "56f8c725-6607-4b3a-aac7-85de9c0b491e";

			DaysOptions = new List<SelectListItem>()
			{
				new SelectListItem() { Text = "Last Hour", Value = "0.0417"},
				new SelectListItem() { Text = "Last 2 Hours", Value = "0.0833"},
				new SelectListItem() { Text = "Last 3 Hours", Value = "0.125"},
				new SelectListItem() { Text = "Last 6 Hours", Value = "0.25"},
				new SelectListItem() { Text = "Last 12 Hours", Value = "0.5"},
				new SelectListItem() { Text = "Last Day", Value = "1"},
				new SelectListItem() { Text = "Last 3 days", Value = "3"},
				new SelectListItem() { Text = "Last Week", Value = "7" },
				new SelectListItem() { Text = "Last Month", Value = "30"},
				new SelectListItem() { Text = "Last 6 Months", Value = "180"},
				new SelectListItem() {Text = "Last Year", Value = "365"},
				new SelectListItem() { Text = "All Time", Value = "0" }
			};
			
			StagesOptions = new List<SelectListItem>()
			{
				new SelectListItem() { Text = "All Mission Stages", Value = "0", Selected = true },
				new SelectListItem() { Text = "Mission Active Stages", Value = "1" }
			};
		}
	}
}