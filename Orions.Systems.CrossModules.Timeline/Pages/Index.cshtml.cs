using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossModules.Timeline.Pages
{
	public class IndexModel : SuperPageModel
	{
		public IndexModel()
		{
			StartTime = null;
			EndTime = null;
		}

		[BindProperty(SupportsGet = true)]
		public string RealmId { get; set; }

		[BindProperty(SupportsGet = true)]
		public string AssetId { get; set; }

		[BindProperty(SupportsGet = true)]
		public string WorkflowInstanceId { get; set; }

		[BindProperty(SupportsGet = true)]
		public string MissionInstanceId { get; set; }

		[BindProperty(SupportsGet = true)]
		public DateTime? StartTime { get; set; }

		[BindProperty(SupportsGet = true)]
		public DateTime? EndTime { get; set; }

		[BindProperty(SupportsGet = true)]
		public bool OrderDescending { get; set; }

		[BindProperty(SupportsGet = true)]
		public string FilterValue { get; set; }

		[BindProperty(SupportsGet = true)]
		public int PageSize { get; set; }

		[BindProperty(SupportsGet = true)]
		public string TimeFilter { get; set; }

		public void OnGetAsync()
		{
			if (Request.Query.Any(it => it.Key == "request"))
			{
				var instructionQuery = Request.Query.FirstOrDefault(it => it.Key == "request");

				string json = System.Uri.UnescapeDataString(instructionQuery.Value);
				var req = new CrossModuleVisualizationRequest();
				JsonHelper.Populate(json, req);

				var wInstIds = req.WorkflowInstanceIds;

				if (wInstIds != null && wInstIds.Any())
				{
					var wInstanceId = wInstIds.First();

					if (!string.IsNullOrEmpty(wInstanceId)) WorkflowInstanceId = wInstanceId;
				}
			}

			ServerUri = TimelineSettings.ServerUri;
		}
	}
}