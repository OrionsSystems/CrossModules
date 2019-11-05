using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orions.Systems.CrossModules.Timeline.Models
{
	public class TagGroupHelper
	{
		public static Dictionary<string, AssetTagGroup> GetTagsByAssetGroups(
			List<DynamicHyperTagViewModel> tags,
			string groupKey)
		{
			var assetGroups = new Dictionary<string, AssetTagGroup>();

			foreach (var tag in tags)
			{
				if (string.IsNullOrEmpty(tag.AssetGuid) && tag.BasicMLTags == null)
					continue;
				try
				{
					var group = FindAssetTagGroup(
						assetGroups,
						tag.AssetGuid,
						tag.MissionName?.ToString(),
						tag.MissionId?.ToString(),
						groupKey);

					group.AddTag(tag.AssetGuid, tag);
				}
				catch (Exception ex)
				{
				}
			}

			return assetGroups;
		}

		private static AssetTagGroup FindAssetTagGroup(
			Dictionary<string, AssetTagGroup> assetGroups,
			string assetId,
			string missionTitle,
			string missionId,
			string groupKey)
		{
			AssetTagGroup group;
			if (!assetGroups.ContainsKey(groupKey))
			{
				group = new AssetTagGroup
				{
					AssetGroupKey = groupKey,
					MissionId = missionId,
					MissionName = missionTitle,
				};
				assetGroups.Add(groupKey, group);
			}
			else
			{
				group = assetGroups[groupKey];
			}

			return (group);
		}

		public static void OrganizeParentChildTags(Dictionary<string, AssetTagGroup> assetGroups)
		{
			foreach (KeyValuePair<string, AssetTagGroup> groupkv in assetGroups)
			{
				groupkv.Value.OrderAssetsByRealWorldTime();

				foreach (KeyValuePair<string, HyperTagGroup> kv in groupkv.Value.AssetToTagList)
				{
					kv.Value.OrganizeParentChildTags();
				}
			}
		}

		public static Stream ExportingToJsonStream(
			Dictionary<string, AssetTagGroup> assetGroups)
		{
			var stringAsStream = new MemoryStream();

			foreach (KeyValuePair<string, AssetTagGroup> groupkv in assetGroups)
			{
				using (StreamWriter sw = new StreamWriter(stringAsStream, Encoding.Unicode))
				{
					sw.WriteLine("{ \"AssetGroupKey\":\"" + groupkv.Value.AssetGroupKey + "\",\n" +
						 "  \"MissionName\":\"" + groupkv.Value.MissionName + "\",\n" +
						 "  \"MissionId\":\"" + groupkv.Value.MissionId + "\",\n" +
						 "  \"HlsSource\":\"" + "HlsSource" + "\",\n" +
						 "  \"Assets\":[");
					int assetcount = 0;
					foreach (KeyValuePair<string, HyperTagGroup> kv in groupkv.Value.AssetToTagList)
					{
						if (assetcount > 0)
							sw.WriteLine(",");
						sw.WriteLine("{\"AssetId\":\"" + kv.Key + "\", \"TagList\":[");
						int taglistcount = 0;
						foreach (DynamicHyperTagViewModel tag in kv.Value.DynamicHelperTags)
						{
							string simpleJSON = tag.ToSimpleJSON("");
							if (taglistcount > 0)
								sw.WriteLine(",");
							sw.Write(simpleJSON);
							taglistcount++;
						}
						sw.Write("] }");
						assetcount++;
					}
					sw.WriteLine();
					sw.WriteLine("] }");
				}
			}
			stringAsStream.Position = 0;
			return stringAsStream;
		}

		public static string ExportingToJsonString(
			Dictionary<string, AssetTagGroup> assetGroups)
		{
			var resultJson = "";

			foreach (KeyValuePair<string, AssetTagGroup> groupkv in assetGroups)
			{
				resultJson += ("{ \"AssetGroupKey\":\"" + groupkv.Value.AssetGroupKey + "\",\n" +
					 "  \"MissionName\":\"" + groupkv.Value.MissionName + "\",\n" +
					 "  \"MissionId\":\"" + groupkv.Value.MissionId + "\",\n" +
					 "  \"HlsSource\":\"" + "HlsSource" + "\",\n" +
					 "  \"Assets\":[");
				int assetcount = 0;
				foreach (KeyValuePair<string, HyperTagGroup> kv in groupkv.Value.AssetToTagList)
				{
					if (assetcount > 0)
						resultJson += ",";
					resultJson += ("{\"AssetId\":\"" + kv.Key + "\", \"TagList\":[");
					int taglistcount = 0;
					foreach (DynamicHyperTagViewModel tag in kv.Value.DynamicHelperTags)
					{
						string simpleJSON = tag.ToSimpleJSON("");
						if (taglistcount > 0)
							resultJson += ",";

						resultJson += simpleJSON;

						taglistcount++;
					}
					resultJson += "] }";
					assetcount++;
				}
				resultJson += Environment.NewLine;
				resultJson += "] }";
			}

			return resultJson;
		}
	}
}
