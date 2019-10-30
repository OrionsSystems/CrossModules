using System;
using System.Collections.Generic;
using System.Linq;

namespace Orions.Systems.CrossModules.Timeline.Models
{
	public class AssetTagGroup
	{
		public string AssetGroupKey = "";
		public string MissionName = "";
		public string MissionId = "";

		public Dictionary<string, HyperTagGroup> AssetToTagList =
			new Dictionary<string, HyperTagGroup>();

		public void OrderAssetsByRealWorldTime()
		{
			AssetToTagList = AssetToTagList
				.OrderBy(x => x.Value.RealWorldContentTimeUTC)
				.ToDictionary(t => t.Key, t => t.Value);
		}

		public void AddTag(string assetId, DynamicHyperTagViewModel tag)
		{
			if (assetId == null)
			{
				throw new ArgumentNullException(nameof(assetId));
			}

			if (tag.FinishedPaths == null && tag.BasicMLTags == null) return;

			HyperTagGroup group;

			if (!AssetToTagList.ContainsKey(assetId))
			{
				group = new HyperTagGroup();
				group.RealWorldContentTimeUTC = tag.RealWorldContentTimeUTC;
				AssetToTagList.Add(assetId, group);
			}
			else
			{
				group = AssetToTagList[assetId];
			}

			group.AddTag(tag);
		}
	}
}
