using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Common;
using Orions.Infrastructure.HyperSemantic;
using Orions.Systems.CrossModules.Components;
using Orions.Infrastructure.Common;

namespace Orions.CrossModules.Data
{
	public class NodeStatisticsManager
	{
		private NetStore _netStore;
		private List<HyperTag> _allTags;
		private List<HyperTag> _filteredTags;

		//private CrossModuleVisualizationRequest _crossModuleVisualizationRequest;

		//private List<FixedCameraEnhancedData> _aois;

		public static int TimingChartSteps = 25;

		public List<TagLabelsPieChartData> TagLabelsData { get; set; } = new List<TagLabelsPieChartData>();

		public List<TagStreamTimeChartData> TagTimingData { get; set; } = new List<TagStreamTimeChartData>();

		public List<TreeMapItem> TagTreemapData { get; set; } = new List<TreeMapItem>();

		public bool IsLoadedData { get; set; }

		public HyperMetadataSet MetadataSet { get; private set; }

		public HyperMission Mission { get; private set; }

		static Lazy<NodeStatisticsManager> _instance = new Lazy<NodeStatisticsManager>();

		public static NodeStatisticsManager Instance => _instance.Value;

		public async Task InitStoreAsync(HyperConnectionSettings connection)
		{
			if (connection != null)
			{
				try
				{
					_netStore = await NetStore.ConnectAsyncThrows(connection.ConnectionUri);
				}
				catch (Exception ex)
				{
					Logger.Instance.Error("Cannot establish the specified connection", ex.Message);
				}
			}			
		}

		public void InitStore(NetStore netStore)
		{
			if (netStore != null)
			{
				_netStore = netStore;
			}
			
		}		

		public async Task InitDataAsync(CrossModuleVisualizationRequest crossModuleVisualizationRequest = null)
		{
			if (_netStore != null)
			{
				var findArgs = new FindHyperDocumentsArgs(typeof(HyperTag))
				{
					Skip = 0,
					Limit = 5000 // TODO: This is limited for faster testing. Consider to increase the value
				};

				if (crossModuleVisualizationRequest != null
					&& crossModuleVisualizationRequest.MetadataSetDocIds != null
					&& crossModuleVisualizationRequest.MetadataSetDocIds.Any())
				{
					var metadataSetId = crossModuleVisualizationRequest.MetadataSetDocIds.FirstOrDefault();

					Logger.Instance.PriorityInfo($"Meta data set id retrieved: {metadataSetId}");
					MetadataSet = await RetrieveHyperDocumentArgs.RetrieveAsync<HyperMetadataSet>(_netStore, metadataSetId);
					await ApplyFilterConditions(findArgs, MetadataSet);

					Mission = await LoadMission(MetadataSet);
				}

				var docs = await _netStore.ExecuteAsync(findArgs) ?? new HyperDocument[0];

				_allTags = docs.Select(x => x.GetPayload<HyperTag>()).ToList();

				PopulateCharts();

				IsLoadedData = true;
			}
			else
			{
				Logger.Instance.Error("NetStore is null");
			}
		}


		public async Task<HyperMission> LoadMission(HyperMetadataSet metadataset)
		{

			var missionId = metadataset?.MissionIds?.FirstOrDefault().Id;

			if (string.IsNullOrWhiteSpace(missionId)) return null;

			var retrieveHyperDocumentArgs = new RetrieveHyperDocumentArgs()
			{
				DocumentId = HyperDocumentId.Create<HyperMission>(missionId)
			};

			var hyperDocument = await _netStore.ExecuteAsync(retrieveHyperDocumentArgs);

			var mission = hyperDocument.GetPayload<HyperMission>();

			return mission;
		}

		private async Task ApplyFilterConditions(FindHyperDocumentsArgs mainArgs, HyperMetadataSet id)
		{
			var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(_netStore, id);
			if (conditions.Result != null)
			{
				mainArgs.DescriptorConditions.AddCondition(conditions.Result);
			}			
		}

		private void PopulateCharts()
		{
			_filteredTags = _allTags;

			PopulateLabelsStatistics();

			PopulateTimingStatistics();

			PopulateTagTreemapData();
		}

		
		private void PopulateLabelsStatistics()
		{
			TagLabelsData.Clear();

			var tagsWithLabels = _filteredTags?.Where(x => x.Elements.Any(el => el is HyperTagLabel)).ToList();

			//var totalTags = tagsWithLabels.Count();

			var labelGroups = tagsWithLabels.GroupBy(x => x.GetElement<HyperTagLabel>().Label).ToList();

			foreach (var labelGroup in labelGroups)
			{
				TagLabelsData.Add(new TagLabelsPieChartData
				{
					Label = labelGroup.Key,
					Count = labelGroup.Count()
				});
			}


			var tagsWithTagonomyResult = _filteredTags?.Where(x => x.Elements.Any(el => el is TagonomyExecutionResultHyperTagElement)).ToList();

			var tagonomyCombinedLabelGroups = tagsWithTagonomyResult.GroupBy(x => x.GetElement<TagonomyExecutionResultHyperTagElement>().GetCombinedLabel()).ToList();

			foreach (var labelGroup in tagonomyCombinedLabelGroups)
			{
				TagLabelsData.Add(new TagLabelsPieChartData
				{
					Label = labelGroup.Key,
					Count = labelGroup.Count()
				});
			}
		}

		private void PopulateTimingStatistics()
		{
			TagTimingData.Clear();

			var timeElements = _filteredTags
				.SelectMany(x => x.GetElements<HyperTagTime>())
				.Where(x => x != null && x.TimeType == HyperTagTime.TimeTypes.StreamTime && x.StreamTime_TimeSpan.HasValue).ToList();

			if (!timeElements.Any())
				return;

			var lowestTime = timeElements.Min(x => x.StreamTime_TimeSpan.Value);
			var highestTime = timeElements.Max(x => x.StreamTime_TimeSpan.Value);

			var timeSpan = highestTime - lowestTime;

			var stepMs = timeSpan.TotalMilliseconds / TimingChartSteps;

			var previousTime = TimeSpan.FromMilliseconds(lowestTime.TotalMilliseconds - 1);
			for (int i = 1; i <= TimingChartSteps; i++)
			{
				var currentTime = TimeSpan.FromMilliseconds(i * stepMs + lowestTime.TotalMilliseconds);

				TagTimingData.Add(new TagStreamTimeChartData
				{
					Time = $"{currentTime.Hours:D2}:{currentTime.Minutes:D2}:{currentTime.Seconds:D2}",
					Count = timeElements.Where(x => x.StreamTime_TimeSpan.Value > previousTime && x.StreamTime_TimeSpan.Value <= currentTime).Count()
				});

				previousTime = currentTime;
			}
		}

		private void PopulateTagTreemapData()
		{
			TagTreemapData.Clear();

			var treeMapItems = new List<TreeMapItem>();

			var mainTreemapItem = new TreeMapItem() { Name = "Tag Labels Map", Value = TagLabelsData.Sum(t => t.Count) };

			foreach (var item in TagLabelsData)
			{
				treeMapItems.Add(new TreeMapItem() { Name = item.LabelInfo, Value = item.Count });
			}
			mainTreemapItem.Items = treeMapItems;
			TagTreemapData.Add(mainTreemapItem);
		}

	}

	public class TagLabelsPieChartData
	{
		public int Count { get; set; }
		public string Label { get; set; }

		public bool ShouldShowInLegend { get; set; } = true;

		public bool Explode { get; set; }

		public string LabelInfo
		{
			get
			{
				return $"{Label}: {Count}";
			}
		}
	}

	public class TagStreamTimeChartData
	{
		public int Count { get; set; }
		public string Time { get; set; }
	}
}
