using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orions.Systems.CrossModules.Timeline.Models
{
	public class HyperTagGroup
	{
		public string BaseTagId;

		public DateTime RealWorldContentTimeUTC = DateTime.MaxValue;

		public List<DynamicHyperTagViewModel> DynamicHelperTags = new List<DynamicHyperTagViewModel>();

		public void AddTag(DynamicHyperTagViewModel newTag)
		{
			AddTag(ref DynamicHelperTags, newTag);
		}

		public static void AddTag(ref List<DynamicHyperTagViewModel> ans, DynamicHyperTagViewModel newTag)
		{
			int insert = 0;
			bool assetFound = false;
			foreach (var t in ans)
			{
				if (t.Tag.AssetGuid == newTag.Tag.AssetGuid) assetFound = true;
				if (assetFound && (t.Tag.AssetGuid != newTag.Tag.AssetGuid)) break;
				if (assetFound && (t.WallClockTime.TotalSeconds >= newTag.WallClockTime.TotalSeconds)) break;
				insert++;
			}
			ans.Insert(insert, newTag);
		}

		public static DynamicHyperTagViewModel FindParentTag(List<DynamicHyperTagViewModel> list, DynamicHyperTagViewModel childTag)
		{
			foreach (DynamicHyperTagViewModel tag in list)
			{
				if (tag.BaseTagId == childTag.ParentTagId) return (tag);
				if (tag.Children != null)
				{
					var childParent = FindParentTag(tag.Children, childTag);
					if (childParent != null) return (childParent);
				}
			}
			return (null);
		}

		public void OrganizeParentChildTags()
		{
			List<DynamicHyperTagViewModel> ans = new List<DynamicHyperTagViewModel>();
			// first, add tags that have no parents...
			foreach (DynamicHyperTagViewModel tag in DynamicHelperTags)
			{
				if (!string.IsNullOrEmpty(tag.ParentTagId)) continue;
				AddTag(ref ans, tag);
			}

			// finer grain sort than the above basic one...
			ans = ans.OrderBy(x => x.AssetGuid)
					  .ThenBy(x => x.WallClockTime.TotalSeconds)
					  .ThenBy(x => x.BaseTagIdIndex).ToList();


			// now add and tags with parents... 
			foreach (DynamicHyperTagViewModel tag in DynamicHelperTags)
			{
				if (string.IsNullOrEmpty(tag.ParentTagId)) continue;
				var parent = FindParentTag(ans, tag);
				if (parent != null)
					parent.Children.Add(tag);
			}

			DynamicHelperTags = ans;
		}
	}
}
