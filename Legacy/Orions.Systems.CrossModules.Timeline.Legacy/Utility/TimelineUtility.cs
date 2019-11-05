using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orions.Systems.CrossModules.Timeline.Models;
using System.Threading;
using Orions.SDK.Utilities;
using Orions.Node.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Common;
using Orions.Infrastructure.HyperSemantic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Orions.Systems.CrossModules.Timeline.Utility
{
	public partial class TimelineUtility
	{
		private readonly IHyperArgsSink _store;

		public TimelineUtility(
			IHyperArgsSink store)
		{
			if (store == null) throw new ArgumentException(nameof(store));

			_store = store;
		}

		public async Task<TimelineViewModel> GetTimeline(
			TagPageFilterModel filter,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			if (filter == null) throw new ArgumentException(nameof(filter));
			if (string.IsNullOrWhiteSpace(filter.ServerUri)) throw new ArgumentException(nameof(filter.ServerUri));

			var tags = await GetTag(filter, cancellationToken);

			var timeline = new TimelineViewModel
			{
				Tags = tags
			};

			GroupCleanAsync(timeline, filter.GroupAndOrganize, cancellationToken);

			return timeline;
		}

		public async Task<List<DynamicHyperTagViewModel>> GetTag(
			TagPageFilterModel filter,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			if (filter == null) throw new ArgumentException(nameof(filter));
			if (string.IsNullOrWhiteSpace(filter.ServerUri)) throw new ArgumentException(nameof(filter.ServerUri));

			var hyperNodeUtility = new HyperStoreUtility(filter.ServerUri, _store);

			var serverHost = HyperStoreConnectionStringUtility.GetHost(filter.ServerUri);
			var serverPort = HyperStoreConnectionStringUtility.GetPort(filter.ServerUri);
			var hlsServerHost = await hyperNodeUtility.GetHlsServerUriAsync(filter.ServerUri);

			var missionTags = new List<DynamicHyperTagViewModel>();

			var findHyperTagsArgs = new FindHyperDocumentsArgs
			{
				DescriptorConditions = new MultiScopeCondition(AndOr.And),
			};

			findHyperTagsArgs.SetDocumentType(nameof(HyperTag));

			var elementType = Assist.GetPropertyName((HyperTag t) => t.Elements) + ".TYPE";
			//findHyperTagsArgs.DescriptorConditions.AddCondition(elementType, nameof(TagonomyExecutionResultHyperTagElement));

			findHyperTagsArgs.OrderByFields = new OrderByField[]
			{
				new OrderByField()
				{
					Ascending = true,
					DescriptorField = true,
					FieldName = "Elements.UniversalTime"
				},
				new OrderByField()
				{
					Ascending = true,
					DescriptorField = true,
					FieldName = "Elements.StreamTime"
				},
				//new OrderByField()
				//{
				//	Ascending = true,
				//	DescriptorField = true,
				//	FieldName = "Elements.HyperId.FragmentId"
				//},
				//new OrderByField()
				//{
				//	Ascending = true,
				//	DescriptorField = true,
				//	FieldName = "Elements.HyperId.SliceId"
				//}
			};

			if (filter.PageNumber.HasValue && filter.PageSize.HasValue)
			{
				findHyperTagsArgs.Skip = filter.PageNumber.Value * filter.PageSize.Value;
				findHyperTagsArgs.Limit = filter.PageSize.Value;
			}

			if (filter.WorkflowInstanceId != null)
			{
				var workflowInstanceKey =
					Assist.GetPropertyName((HyperTag t) => t.Elements) + "." +
					Assist.GetPropertyName((HyperTagMission m) => m.WorkflowInstanceId);

				findHyperTagsArgs.DescriptorConditions.AddCondition(workflowInstanceKey, filter.WorkflowInstanceId);
			}

			if (filter.Ids != null && filter.Ids.Any())
			{
				findHyperTagsArgs.DocumentConditions.AddCondition("_id", filter.Ids, Comparers.In);
			}

			if (filter.AssetIds != null && filter.AssetIds.Any())
			{
				var assetDescriptorConditions = new MultiScopeCondition(AndOr.Or);
				foreach (var assetId in filter.AssetIds)
				{
					var regexText = $"/^{assetId}.*/i";

					var key =
						Assist.GetPropertyName((HyperTag t) => t.Elements) + $".{nameof(HyperId)}." +
						Assist.GetPropertyName((HyperId m) => m.AssetId);

					assetDescriptorConditions.AddCondition(key, regexText, Comparers.Regex);
				}
				findHyperTagsArgs.DescriptorConditions.AddCondition(assetDescriptorConditions);
			}

			if (filter.Start.HasValue || filter.End.HasValue)
			{
				var field = Assist.GetPropertyName((HyperTag t) => t.Elements)
					+ "."
					+ Assist.GetPropertyName((HyperTagTime t) => t.UniversalTime);

				if (filter.Start.HasValue)
				{
					findHyperTagsArgs.DescriptorConditions.AddCondition(
						field,
						new DateTime(filter.Start.Value.Ticks, DateTimeKind.Utc),
						Comparers.GreaterThanOrEqual);
				}

				if (filter.End.HasValue)
				{
					findHyperTagsArgs.DescriptorConditions.AddCondition(
						field,
						new DateTime(filter.End.Value.Ticks, DateTimeKind.Utc),
						Comparers.LessThanOrEqual);
				}
			}

			if (filter.Range != null && filter.AssetIds != null && filter.AssetIds.Any())
			{
				var assetId = filter.AssetIds.FirstOrDefault();

				var hyperAssetId = HyperAssetId.TryParse(assetId);

				// TODO: Enable cache
				// var hyperTrack = _tagsCache.Get<HyperTrack>(assetId);
				HyperTrack hyperTrack = null;

				if (hyperTrack == null)
				{
					var retrieveAssetArgs = new RetrieveAssetArgs()
					{
						AssetId = hyperAssetId.Value,
					};

					var hyperAsset = await _store.ExecuteAsync(retrieveAssetArgs);

					var retrieveTrackArgs = new RetrieveTrackArgs()
					{
						AssetId = hyperAssetId.Value,
						TrackId = hyperAsset.DefaultVideoTrackId.Value
					};

					hyperTrack = await _store.ExecuteAsync(retrieveTrackArgs);

					// TODO: Enable cache
					//_tagsCache.Set(assetId, hyperTrack, TimeSpan.FromSeconds(Settings.Instance.CacheExpiratonTagIntervalInSeconds));
				}

				var fromExtended = await hyperTrack.FindAtAsync(_store, hyperAssetId.Value, filter.Range.FromSeconds * 1000, HyperSeek.SeekModes.Estimated);

				var toExtended = await hyperTrack.FindAtAsync(_store, hyperAssetId.Value, filter.Range.ToSeconds * 1000, HyperSeek.SeekModes.Estimated);

				var field = Assist.GetPropertyName((HyperTag t) => t.Elements)
					+ $".{nameof(HyperId)}."
					+ Assist.GetPropertyName((HyperId t) => t.FragmentId);

				findHyperTagsArgs.DescriptorConditions.AddCondition(
					field,
					fromExtended.Value.FragmentId.Index,
					Comparers.GreaterThanOrEqual);

				findHyperTagsArgs.DescriptorConditions.AddCondition(
					field,
					toExtended.Value.FragmentId.Index,
					Comparers.LessThanOrEqual);
			};

			var documents = await _store.ExecuteAsync(findHyperTagsArgs);

			if (documents == null) return missionTags;

			var parallel = documents
				.Select(it => it.GetPayload<HyperTag>())
				.Where(it => it.GetElement<HyperTagMission>() != null)
				.ToDictionary(k => k, v => new DynamicHyperTagViewModel());

			if (!parallel.Any()) return missionTags;

			foreach (var item in parallel.AsParallel().WithDegreeOfParallelism(100))
			{
				try
				{
					// TODO: Enable cache
					//var cacheElement = _tagsCache.Get<TagCacheElement>(item.Key.Id);
					TagCacheElement cacheElement = null;

					if (cacheElement != null)
					{
						var cacheTag = cacheElement.Tag;

						item.Value.ServerUri = cacheTag.ServerUri;
						item.Value.ServerHost = cacheTag.ServerHost;
						item.Value.ServerPort = cacheTag.ServerPort;
						item.Value.HlsServerHost = cacheTag.HlsServerHost;
						item.Value.DynamicData = cacheTag.DynamicData;
						item.Value.TagView = cacheTag.TagView;
						item.Value.HyperTag = cacheTag.HyperTag;
					}
					else
					{
						var view = await HyperTagHelper.GenerateTagView(_store, new DummyLogger(), item.Key);

						var json = JsonConvert.SerializeObject(view);
						var @object = JsonConvert.DeserializeObject<JObject>(json);

						item.Value.ServerUri = filter.ServerUri;
						item.Value.ServerHost = serverHost;
						item.Value.ServerPort = serverPort;
						item.Value.HlsServerHost = hlsServerHost;
						item.Value.DynamicData = @object;
						item.Value.TagView = view;
						item.Value.HyperTag = item.Key;

						cacheElement = new TagCacheElement()
						{
							Tag = item.Value,
						};

						// TODO: Enable cache
						//_tagsCache.Set(item.Key.Id, cacheElement, TimeSpan.FromSeconds(Settings.Instance.CacheExpiratonTagIntervalInSeconds));
					}
				}
				catch (Exception)
				{
				}
			}

			missionTags.AddRange(parallel.Where(it => it.Value.DynamicData != null).Select(it => it.Value));

			if (!string.IsNullOrWhiteSpace(filter.FilterValue))
			{
				missionTags = missionTags.Where(it => it.TagView.TaggingTitle != null &&
					it.TagView.TaggingTitle.ToLower().Contains(filter.FilterValue.ToLower())).ToList();
			}

			return missionTags;
		}

		public async Task<long> CountTag(
			TagPageFilterModel filter,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			if (filter == null) throw new ArgumentException(nameof(filter));
			if (string.IsNullOrWhiteSpace(filter.ServerUri)) throw new ArgumentException(nameof(filter.ServerUri));

			var findHyperTagsArgs = new CountHyperDocumentsArgs
			{
				DescriptorConditions = new MultiScopeCondition(AndOr.And),
			};

			var elementType = Assist.GetPropertyName((HyperTag t) => t.Elements) + ".TYPE";
			findHyperTagsArgs.DescriptorConditions.AddCondition(elementType, nameof(TagonomyExecutionResultHyperTagElement));

			if (!filter.Children)
			{
				findHyperTagsArgs.DescriptorConditions.AddCondition(elementType, nameof(HyperTagReference), Comparers.DoesNotEqual);
			}
			else
			{
				findHyperTagsArgs.DescriptorConditions.AddCondition(elementType, nameof(HyperTagReference), Comparers.Equals);
				var elementRefrenceId = Assist.GetPropertyName((HyperTag t) => t.Elements) + ".ReferenceId";
				findHyperTagsArgs.DescriptorConditions.AddCondition(elementRefrenceId, filter.ParentId, Comparers.Equals);
			}

			findHyperTagsArgs.OrderByFields = new OrderByField[]
			{
				new OrderByField()
				{
					Ascending = false,
					DescriptorField = true,
					FieldName = "Elements.HyperId.FragmentId"
				},
				new OrderByField()
				{
					Ascending = false,
					DescriptorField = true,
					FieldName = "Elements.HyperId.SliceId"
				}
			};

			findHyperTagsArgs.SetDocumentType(nameof(HyperTag));

			if (filter.WorkflowInstanceId != null)
			{
				var workflowInstanceKey =
					Assist.GetPropertyName((HyperTag t) => t.Elements) + "." +
					Assist.GetPropertyName((HyperTagMission m) => m.WorkflowInstanceId);

				findHyperTagsArgs.DescriptorConditions.AddCondition(workflowInstanceKey, filter.WorkflowInstanceId);
			}

			if (filter.Ids != null && filter.Ids.Any())
			{
				findHyperTagsArgs.DocumentConditions.AddCondition("_id", filter.Ids, Comparers.In);
			}

			if (filter.AssetIds != null || filter.AssetIds.Any())
			{
				var key =
					Assist.GetPropertyName((HyperTag t) => t.Elements) + $".{nameof(HyperId)}." +
					Assist.GetPropertyName((HyperId m) => m.AssetId);

				findHyperTagsArgs.DocumentConditions.AddCondition(key, filter.AssetIds, Comparers.In);
			}

			if (filter.Start.HasValue || filter.End.HasValue)
			{
				var field = Assist.GetPropertyName((HyperTag t) => t.Elements)
					+ "."
					+ Assist.GetPropertyName((HyperTagTime t) => t.UniversalTime);

				if (filter.Start.HasValue)
				{
					findHyperTagsArgs.DescriptorConditions.AddCondition(
						field,
						new DateTime(filter.Start.Value.Ticks, DateTimeKind.Utc),
						Comparers.GreaterThanOrEqual);
				}

				if (filter.End.HasValue)
				{
					findHyperTagsArgs.DescriptorConditions.AddCondition(
						field,
						new DateTime(filter.End.Value.Ticks, DateTimeKind.Utc),
						Comparers.LessThanOrEqual);
				}
			}

			var documents = await _store.ExecuteAsync(findHyperTagsArgs);

			return documents;
		}

		public async Task<DynamicHyperTagViewModel> GetTag(
			TagItemFilterModel filter,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			if (filter == null) throw new ArgumentException(nameof(filter));
			if (string.IsNullOrWhiteSpace(filter.ServerUri)) throw new ArgumentException(nameof(filter.ServerUri));

			var hyperNodeUtility = new HyperStoreUtility(filter.ServerUri, _store);

			var serverHost = HyperStoreConnectionStringUtility.GetHost(filter.ServerUri);
			var serverPort = HyperStoreConnectionStringUtility.GetPort(filter.ServerUri);
			var hlsServerHost = await hyperNodeUtility.GetHlsServerUriAsync(filter.ServerUri);

			var missionTags = new List<DynamicHyperTagViewModel>();

			var tagArgs = new RetrieveHyperDocumentArgs()
			{
				DocumentId = new HyperDocumentId(filter.Id, typeof(HyperTag)),
			};

			var document = await _store.ExecuteAsync(tagArgs);

			if (document == null) return null;

			var hyperTag = document.GetPayload<HyperTag>();

			// TODO: Enable cache
			// var cacheElement = _tagsCache.Get<TagCacheElement>(hyperTag.Id);
			TagCacheElement cacheElement = null;

			var dynamicTag = new DynamicHyperTagViewModel();

			if (cacheElement != null)
			{
				var cacheTag = cacheElement.Tag;

				dynamicTag.ServerUri = cacheTag.ServerUri;
				dynamicTag.ServerHost = cacheTag.ServerHost;
				dynamicTag.ServerPort = cacheTag.ServerPort;
				dynamicTag.HlsServerHost = cacheTag.HlsServerHost;
				dynamicTag.DynamicData = cacheTag.DynamicData;
				dynamicTag.TagView = cacheTag.TagView;
				dynamicTag.HyperTag = cacheTag.HyperTag;
			}
			else
			{
				var view = await HyperTagHelper.GenerateTagView(_store, new DummyLogger(), hyperTag);

				var json = JsonHelper.Serialize(view, false);
				var @object = JsonHelper.Deserialize(json);

				dynamicTag.ServerUri = filter.ServerUri;
				dynamicTag.ServerHost = serverHost;
				dynamicTag.ServerPort = serverPort;
				dynamicTag.HlsServerHost = hlsServerHost;
				dynamicTag.DynamicData = @object;
				dynamicTag.TagView = view;
				dynamicTag.HyperTag = hyperTag;

				cacheElement = new TagCacheElement()
				{
					Tag = dynamicTag,
				};

				// TODO: Enable cache
				//_tagsCache.Set(hyperTag.Id, cacheElement, TimeSpan.FromSeconds(Settings.Instance.CacheExpiratonTagIntervalInSeconds));
			}

			return dynamicTag;
		}

		private void GroupCleanAsync(
			TimelineViewModel timeline,
			bool group,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			if (timeline == null) throw new ArgumentException(nameof(timeline));

			timeline.Tags = GroupTagsByTime(timeline);

			timeline.Tags = timeline.Tags.Where(it => it.WallClockTime != null).Select(it => it).ToList();
			timeline.Tags.Sort((x, y) => x.WallClockTime.CompareTo(y.WallClockTime));

			timeline.TagAssetIds = timeline.Tags
				.Where(it => !string.IsNullOrWhiteSpace(it.AssetGuid))
				.Select(it => it.AssetGuid).Distinct().ToList();

			if (group)
			{
				foreach (var tag in timeline.Tags)
				{
					//NOTE clean children when tags come from cache
					tag.Children = new List<DynamicHyperTagViewModel>();
				}

				timeline.Groups = TagGroupHelper.GetTagsByAssetGroups(timeline.Tags, "group");

				TagGroupHelper.OrganizeParentChildTags(timeline.Groups);

				timeline.Json = TagGroupHelper.ExportingToJsonString(timeline.Groups);
			}

			if (string.IsNullOrWhiteSpace(timeline.Json))
			{
				timeline.Json = "";
				var index = 0;
				foreach (var tag in timeline.Tags)
				{
					if (index == 0)
					{
						timeline.Json += tag.ToSimpleJSON("");
					}
					else
					{
						timeline.Json += "," + tag.ToSimpleJSON("");
					}
					index++;
				}
				timeline.Json = "[" + timeline.Json + "]";
			}
		}

		private List<DynamicHyperTagViewModel> GroupTagsByTime(
			TimelineViewModel timeline)
		{
			var groupedTags = new List<DynamicHyperTagViewModel>();
			var items = timeline.Tags.GroupBy(it => new { it.WallClockTime, it.AssetGuid }, (key, data) => new { Key = key, Items = data });
			foreach (var tagGroup in items)
			{
				var data = tagGroup.Items;
				var parrentTags = data.Where(it => string.IsNullOrEmpty(it.ParentTagId));
				groupedTags.Add(parrentTags.FirstOrDefault());

				var childrenTags = data.Where(it => !string.IsNullOrEmpty(it.ParentTagId));
				groupedTags.AddRange(childrenTags);
			}
			return groupedTags;
		}

		private class TagCacheElement
		{
			public DynamicHyperTagViewModel Tag;
		}

		private class TimelineCacheElement
		{
			public TagPageFilterModel Filter;
			public TimelineViewModel Timeline;
		}
	}
}
