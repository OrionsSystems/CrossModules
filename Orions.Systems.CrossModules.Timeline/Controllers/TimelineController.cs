using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using Microsoft.AspNetCore.Mvc;

using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.SDK.Utilities;
using Orions.XPlatform;

using TagPageFilterModel = Orions.XPlatform.TagPageFilterModel;

namespace Orions.Systems.CrossModules.Timeline.Controllers
{
	public class TimelineController : SuperController
	{
		public async Task<IActionResult> Timeline_Read(
			string missionInstanceId,
			string workflowInstanceId,
			string assetId,
			string filterValue,
			string realmId,
			DateTime? startTime,
			DateTime? endTime,
			ulong? fromSeconds = null,
			ulong? toSeconds = null,
			int pageNumber = 0,
			int pageSize = 20)
		{
			var filter = new TagPageFilterModel()
			{
				ServerUri = ServerUri,
				PageSize = pageSize,
				PageNumber = pageNumber,
				AssetIds = new List<string>() { assetId },
				Start = startTime,
				End = endTime,
				GroupAndOrganize = true,
				WorkflowInstanceId = workflowInstanceId,
				MissionInstanceId = missionInstanceId,
				FilterValue = filterValue,
				RealmId = realmId,
			};

			if (fromSeconds.HasValue && toSeconds.HasValue)
			{
				filter.Range = new TagPageFilterModel.PositionRange(fromSeconds.Value, toSeconds.Value);
			}

			var timeline = new TimelineViewModel();

			var service = new NodeTimelineXService(TimelineSettings.NodeInfo);

			timeline = await service.GetTimeline(filter);

			if (timeline.Tags.Any())
			{
				ViewBag.IsChildren = false;
				return PartialView("_timelineTree", timeline.Tags);
			}

			return Content("");
		}

		public async Task<ActionResult> Timeline_AssetRead(
			[DataSourceRequest] DataSourceRequest dataRequest,
			List<string> ids)
		{
			var request = new AssetPageRequest
			{
				PageNumber = dataRequest.Page - 1,
				PageSize = dataRequest.PageSize,
				ServerUri = ServerUri,
				TagId = TagId,
				Full = true
			};

			var model = new AssetPageModel(request);
			if (ids == null || !ids.Any())
			{
				return Json(model.Items.ToDataSourceResult(dataRequest));
			}

			request.Ids = ids.Distinct().ToList();
			
			var service = new AssetXService(CompanyId, Authentication);

			model = await service.GetAsync(request);

			foreach (var item in model.Items)
			{
				item.HyperAsset.MetaData.Default.SetDefault((HyperSystemTag)null);
			}

			return Json(model.Items.ToDataSourceResult(dataRequest));
		}

		[HttpPost]
		public async Task<IActionResult> Timeline_Children(
			string missionInstanceId,
			string workflowInstanceId,
			string realmId,
			string parentId)
		{
			var timeline = new TimelineViewModel();

			var filter = new TagPageFilterModel()
			{
				ServerUri = ServerUri,
				RealmId = realmId,
				MissionInstanceId = missionInstanceId,
				WorkflowInstanceId = workflowInstanceId,
				Children = true,
				GroupAndOrganize = true,
				ParentId = parentId
			};

			var tags = new List<DynamicHyperTagViewModel>();

			if (string.IsNullOrWhiteSpace(realmId))
			{
				var service = new NodeTimelineXService(CompanyId, Authentication);

				tags = await service.GetTag(filter);
			}
			else
			{
				var service = new CloudTimelineXService(CompanyId, Authentication);

				tags = await service.GetTag(filter);
			}

			tags = tags.OrderByDescending(it => it.RealWorldContentTimeUTC).ToList();

			ViewBag.IsChildren = true;

			return PartialView("_timelineTree", tags);
		}

		public async Task<IActionResult> Timeline_CountChildren(
			string missionInstanceId,
			string workflowInstanceId,
			List<string> assetIds,
			string filterValue,
			string realmId,
			string parentId,
			DateTime? startTime,
			DateTime? endTime)
		{
			var filter = new TagPageFilterModel()
			{
				ServerUri = ServerUri,
				AssetIds = assetIds,
				Start = startTime,
				End = endTime,
				WorkflowInstanceId = workflowInstanceId,
				MissionInstanceId = missionInstanceId,
				FilterValue = filterValue,
				RealmId = realmId,

				Children = true,
				ParentId = parentId
			};

			if (string.IsNullOrWhiteSpace(realmId))
			{
				var service = new NodeTimelineXService(CompanyId, Authentication);

				var result = await service.CountTag(filter);

				return Content(result.ToString());
			}
			else
			{
				//var service = new CloudTimelineXService(CompanyId, Authentication);

				//timeline = await service.GetTimeline(filter);				
			}

			return Content("0");
		}

		public async Task<IActionResult> Timeline_Elements(
			string id,
			string realmId)
		{
			var filter = new TagItemFilterModel(id)
			{
				ServerUri = ServerUri,
				RealmId = realmId
			};

			DynamicHyperTagViewModel tag;
			var json = "";
			if (string.IsNullOrWhiteSpace(realmId))
			{
				var service = new NodeTimelineXService(CompanyId, Authentication);

				tag = await service.GetTag(filter);

				json = JsonHelper.Serialize(tag?.HyperTag.Elements);
			}
			else
			{
				var service = new CloudTimelineXService(CompanyId, Authentication);

				tag = await service.GetTag(filter);

				var elements = tag.SafeGetElementsFromJson();

				json = JsonHelper.Serialize(elements);
			}

			return Content(json);
		}
	}
}
