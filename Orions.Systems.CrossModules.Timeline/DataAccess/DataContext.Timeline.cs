using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orions.Infrastructure.HyperMedia;
using Orions.SDK.Utilities;
using Orions.Systems.CrossModules.Timeline.Models;
using Orions.Systems.CrossModules.Timeline.Utility;

namespace Orions.Systems.CrossModules.Timeline
{
	public partial class DataContext
	{
		public async Task<TimelineViewModel> GetTimeline(
			FilterViewModel filter)
		{
			if (!(!string.IsNullOrWhiteSpace(WorkflowInstanceId)
				|| !string.IsNullOrWhiteSpace(AssetId))) return new TimelineViewModel();

			var model = new TagPageFilterModel()
			{
				ServerUri = ServerUri,
				PageSize = filter.PageSizeValue,
				PageNumber = filter.PageNumber,
				AssetIds = new List<string>() { AssetId },
				Start = filter.StartTime,
				End = filter.EndTime,
				GroupAndOrganize = true,
				WorkflowInstanceId = WorkflowInstanceId,
				MissionInstanceId = MissionInstanceId,
				FilterValue = filter.Input,
				//RealmId = realmId,
			};

			if (filter.FromSeconds.HasValue && filter.ToSeconds.HasValue)
			{
				model.Range = new TagPageFilterModel.PositionRange(filter.FromSeconds.Value, filter.ToSeconds.Value);
			}

			var utility = new TimelineUtility(_netStore);

			return await utility.GetTimeline(model);
		}

		public async Task<IEnumerable<AssetModel>> GetTimelineAsset(
			AssetPageFilter filter)
		{
			var request = new AssetPageRequest
			{
				PageNumber = filter.Page - 1,
				PageSize = filter.PageSize,
				ServerUri = ServerUri,
				//TagId = TagId,
				Full = true
			};

			var model = new AssetPageModel(request);
			if (filter.AssetIds == null || !filter.AssetIds.Any())
			{
				return model.Items.AsQueryable();
			}

			request.Ids = filter.AssetIds.Distinct().ToList();

			var utility = new AssetUtility(GetStores());

			model = await utility.GetAsync(request);

			foreach (var item in model.Items)
			{
				item.HyperAsset.MetaData.Default.SetDefault((HyperSystemTag)null);
			}

			return model.Items.AsQueryable();
		}

		public async Task<List<DynamicHyperTagViewModel>> Timeline_Children(
			string missionInstanceId,
			string workflowInstanceId,
			string parentId)
		{
			var timeline = new TimelineViewModel();

			var filter = new TagPageFilterModel()
			{
				ServerUri = ServerUri,
				//RealmId = realmId,
				MissionInstanceId = missionInstanceId,
				WorkflowInstanceId = workflowInstanceId,
				Children = true,
				GroupAndOrganize = true,
				ParentId = parentId
			};

			var utility = new TimelineUtility(_netStore);
			var tags = await utility.GetTag(filter);

			tags = tags.OrderByDescending(it => it.RealWorldContentTimeUTC).ToList();

			return tags;
		}

		public async Task<long> Timeline_CountChildren(
			string missionInstanceId,
			string workflowInstanceId,
			List<string> assetIds,
			string filterValue,
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
				//RealmId = realmId,

				Children = true,
				ParentId = parentId
			};

			var utility = new TimelineUtility(_netStore);

			return await utility.CountTag(filter);
		}

		public async Task<IEnumerable<IHyperTagElement>> Timeline_Elements(
			string id,
			string realmId)
		{
			var filter = new TagItemFilterModel(id)
			{
				ServerUri = ServerUri,
				RealmId = realmId
			};

			var utility = new TimelineUtility(_netStore);

			var tag = await utility.GetTag(filter);

			return tag?.HyperTag.Elements.AsQueryable();
		}
	}
}
