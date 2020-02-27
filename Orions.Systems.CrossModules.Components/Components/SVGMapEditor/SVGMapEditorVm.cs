﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

using Microsoft.JSInterop;

using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperMedia.MapOverlay;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Components.Components.SVGMapEditor.JsModel;

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor
{
	public class SVGMapEditorVm : BlazorVm
	{
		private Helpers.MasksHeatmapRenderer.RenderingMode _heatmapRendererMode => HeatmapRenderingModeToRenderersModes(HeatmapMode);

		private Helpers.MasksHeatmapRenderer _heatmapRenderer;

		#region Properties
		public IHyperArgsSink HyperArgsSink { get; set; }
		public IJSRuntime JsRuntime { get; set; }
		public HyperDocumentId? MapOverlayId { get; set; }
		public HyperDocumentId? MetadataSetId { get; set; }
		public ViewModelProperty<MapOverlay> MapOverlay { get; set; } = new ViewModelProperty<MapOverlay>(new Infrastructure.HyperMedia.MapOverlay.MapOverlay());
		public Func<HyperDocumentId?, Task> OnMapOverlayIdSet { get; set; }
		public Func<MapPlaybackCache, Task> OnMapPlayebackCacheUpdated { get; set; }
		public Func<TagDateRangeFilterOptions, Task> TagDateRangeFilterChanged { get; set; }

		public string DefaultCircleColor { get; internal set; }
		public string DefaultZoneColor { get; internal set; }
		public string DefaultCameraColor { get; internal set; }
		public bool IsReadOnly { get; set; }

		private int _tagRequestMaxCountLimit = 250;
		public int TagRequestMaxCountLimit { get { return _tagRequestMaxCountLimit; } set { if (value > 0) _tagRequestMaxCountLimit = value; } }

		public ViewModelProperty<bool> ShowingControlPropertyGrid { get; set; } = false;
		public OverlayEntry CurrentPropertyGridObject { get; set; }
		public ViewModelProperty<bool> ShowingHyperTagInfo { get; set; } = false;
		public ViewModelProperty<HyperTag> CurrentTagBeingShown { get; set; } = new ViewModelProperty<HyperTag>();
		public double HyperTagInfoXPos { get; set; }
		public double HyperTagInfoYPos { get; set; }
		public ViewModelProperty<bool> ShowingHyperTagProperties { get; set; } = false;

		public ViewModelProperty<bool> IsVmShowingHeatmapProp { get; set; } = new ViewModelProperty<bool>(false);

		public ViewModelProperty<string> HeatmapImgProp { get; set; } = new ViewModelProperty<string>();
		public ZoneDataSet CurrentlyShownHeatmapZoneDataSet { get; set; }

		public HeatmapRenderingMode HeatmapMode { get; set; }
		public bool HeatmapCustomNormalization { get; set; }
		public uint HeatmapNormalizationMinOverlaps { get; set; }
		public uint HeatmapNormalizationMaxOverlaps { get; set; }

		public string TagInfoImageBase64Url
		{
			get
			{
				if (TagInfoImage.Value != null)
				{
					return $"data:image/jpg;base64, {Convert.ToBase64String(TagInfoImage.Value)}";
				}

				return null;
			}
		}

		public ViewModelProperty<byte[]> TagInfoImage { get; set; } = new ViewModelProperty<byte[]>();

		public ViewModelProperty<bool> TagsAreBeingLoaded { get; set; } = new ViewModelProperty<bool>(false);

		public bool TagDateFilterPreInitialized { get; set; } = false;
		public TagDateRangeFilterOptions TagDateRangeFilter { get; set; } = new TagDateRangeFilterOptions();
		public TagDateRangeFilterOptions AutoplayTagDateRangeFilter { get; set; } = new TagDateRangeFilterOptions();
		public ViewModelProperty<bool> HomographiesDetected { get; set; } = false;
		public ViewModelProperty<bool> IsAutoPlayOn { get; set; } = false;
		public MapPlaybackOptions PlaybackOptions { get; set; }
		public ViewModelProperty<MapPlaybackCache> PlaybackCache { get; set; }
		public ViewModelProperty<bool> PlaybackCacheBeingUpdated { get; set; } = false;

		public bool HeatmapAvailableForSelectedZone
		{
			get
			{
				var heatmapAvailable = false;
				if (!string.IsNullOrWhiteSpace(CurrentlySelectedZoneId))
				{
					var zoneDataSet = GetZoneDataSetForCurrentShownZone(CurrentlySelectedZoneId);
					if (zoneDataSet != null)
					{
						if (zoneDataSet.Zone.FixedCameraEnhancementId != null && !string.IsNullOrWhiteSpace(zoneDataSet.Zone.Alias) && zoneDataSet.Zone.MetadataSetId != null
							&& zoneDataSet.Tags != null && zoneDataSet.Tags.Any())
						{
							heatmapAvailable = true;
						}
						else
						{
							heatmapAvailable = false;
						}
					}
					else
					{
						heatmapAvailable = false;
					}
				}
				else
				{
					heatmapAvailable = false;
				}

				return heatmapAvailable;
			}
		}

		public string CurrentlySelectedZoneId { get; set; }
		#endregion // Properties

		private List<ZoneDataSet> ZoneDataSets = new List<ZoneDataSet>();

		private string _componentContainerId;


		public SVGMapEditorVm()
		{
			PlaybackOptions = new MapPlaybackOptions()
			{
				PlayStep = TimeSpan.FromSeconds(3600),
				PlayDuration = TimeSpan.FromSeconds(5),
				LoadMode = MapPlaybackOptions.LoadModeEnum.Cache
			};
		}

		#region Methods
		#region Heatmap
		public async Task OpenHeatmapAsync(string zoneId)
		{
			await OpenPopupMap(zoneId, true);

			await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.makePopupDraggable", new object[] { _componentContainerId });
		}

		public async Task OpenRealMasksMapAsync(string zoneId)
		{
			await OpenPopupMap(zoneId, false);
		}

		private Helpers.MasksHeatmapRenderer.RenderingMode HeatmapRenderingModeToRenderersModes(HeatmapRenderingMode mode)
		{
			switch (mode)
			{
				case HeatmapRenderingMode.Masks:
					return Helpers.MasksHeatmapRenderer.RenderingMode.Masks;
				case HeatmapRenderingMode.LowerPointOfGeometry:
					return Helpers.MasksHeatmapRenderer.RenderingMode.LowerPointOfGeometry;
				default:
					return Helpers.MasksHeatmapRenderer.RenderingMode.LowerPointOfGeometry; // Most compatible
			}
		}

		private async Task OpenPopupMap(string zoneId, bool heatmapMode)
		{
			var zoneDataSet = GetZoneDataSetForCurrentShownZone(zoneId);
			
			if(zoneDataSet != null)
			{
				this.CurrentlyShownHeatmapZoneDataSet = zoneDataSet; // remember requested zoneId dataset to enable live heatmap refresh while autoplayback is on

				var tagsForMap = zoneDataSet.Tags;
				var fixedCameraEnhancementId = zoneDataSet.Zone.FixedCameraEnhancementId;

				if (zoneDataSet.Heatmap != null)
				{
					this.HeatmapImgProp.Value = zoneDataSet.Heatmap.DataBase64Link;
				}
				else
				{
					PrepareHeatmapAsyncFunc(tagsForMap, fixedCameraEnhancementId.Value, heatmapMode);
				}

				IsVmShowingHeatmapProp.Value = true;
			}
		}

		public async Task PrepareHeatmapAsyncFunc(List<HyperTag> tagsForMap, HyperDocumentId fixedCameraEnhancementId, bool heatmapMode)
		{
			_heatmapRenderer = InstantiateHeatmapRendererWithSettings();
			var img = await _heatmapRenderer.GenerateFromTagsAsync(tagsForMap, fixedCameraEnhancementId, false);

			if (img != null)
			{
				HeatmapImgProp.Value = img.DataBase64Link;
			}
		}

		private ZoneDataSet GetZoneDataSetForCurrentShownZone(string zoneId)
		{
			ZoneDataSet zoneDataSet;
			if (this.IsAutoPlayOn && this.PlaybackOptions.LoadMode == MapPlaybackOptions.LoadModeEnum.Cache)
			{

				zoneDataSet = this.PlaybackCache.Value.Steps.SingleOrDefault(s => s.From == this.AutoplayTagDateRangeFilter.CurrentMinDate && s.To == this.AutoplayTagDateRangeFilter.CurrentMaxDate)?.ZoneDataSets.SingleOrDefault(ds => ds.Zone.Id == zoneId);
			}
			else
			{
				zoneDataSet = ZoneDataSets.SingleOrDefault(z => z.Zone.Id == zoneId);
			}

			return zoneDataSet;
		}

		public void CloseHeatmap()
		{
			IsVmShowingHeatmapProp.Value = false;
			HeatmapImgProp.Value = null;
			_heatmapRenderer?.Dispose();
		}
		#endregion // Heatmap

		#region Initializing
		public async Task Initialize(string componentContainerId, DotNetObjectReference<SVGMapEditorBase> thisReference)
		{
			try
			{
				this._componentContainerId = componentContainerId;
				if (MapOverlayId != null)
				{
					var retrieveArgs = new RetrieveHyperDocumentArgs(MapOverlayId.Value);

					var hyperDocument = await HyperArgsSink.ExecuteAsync(retrieveArgs);

					var mapOverlay = hyperDocument.GetPayload<MapOverlay>();

					this.MapOverlay.Value = mapOverlay;
				}
				else
				{
					var doc = new HyperDocument(MapOverlay.Value);

					var storeDocArgs = new StoreHyperDocumentArgs(doc);

					await this.HyperArgsSink.ExecuteAsync(storeDocArgs);

					if (this.OnMapOverlayIdSet != null)
					{
						await OnMapOverlayIdSet.Invoke(doc.Id);
					}
				}

				var editorConfig = new SvgEditorConfig
				{
					CameraColor = this.DefaultCameraColor,
					ZoneColor = this.DefaultZoneColor,
					CircleColor = this.DefaultCircleColor,
					IsReadOnly = this.IsReadOnly
				};

				var overlayJsModel = MapOverlayJsModel.CreateFromDomainModel(this.MapOverlay.Value);
				foreach (var zone in overlayJsModel.Zones)
				{
					zone.EventHandlerMappings.Add("startResize", "RemoveTagCirclesForZone");
					zone.EventHandlerMappings.Add("zoneIsBeingDragged", "RemoveTagCirclesForZone");
					zone.EventHandlerMappings.Add("zoneHasBeenDragged", "UpdateZone");
					zone.EventHandlerMappings.Add("zoneHasBeenResized", "UpdateZone");
					zone.EventHandlerMappings.Add("zoneSelected", "SelectZone");
					zone.EventHandlerMappings.Add("zoneLostSelection", "UnselectZone");
				}

				await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.init", new object[] { componentContainerId, thisReference, overlayJsModel, editorConfig });

				this.HomographiesDetected.Value = this.GetMapOverlayZonesWithHomographyAssigned().Any();
				if (this.HomographiesDetected)
				{
					if (this.TagDateFilterPreInitialized)
					{
						InitializeTagFilter();
					}
					else
					{
						await InitializeTagFilter();
					}
				}

				await ShowTags();

			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Assert(false, ex.Message);
			}
		}

		public async Task RemoveTagCirclesForZone(string zoneId)
		{
			var circlesToRemove = _circlesToTagsMappings.Where(c => ZoneDataSets.Any(z => z.Tags.Any(t => t.Id == c.Value.Id) && z.Zone.Id == zoneId)).Select(kv => new MapOverlayUpdateDetails
			{
				Type = MapOverlayUpdateDetails.DeleteUpdateType,
				OverlayEntry = JsonSerializer.Serialize(kv.Key, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
			}).ToArray();

			if (circlesToRemove.Any())
			{
				await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, circlesToRemove, false, "batch" });
			}
		}

		private async Task InitializeTagFilter()
		{
			this.TagsAreBeingLoaded.Value = !this.TagDateFilterPreInitialized;
			if (this.TagDateFilterPreInitialized)
			{
				this.InitializeAutoplayTagDateRangeFilter();
			}

			this.TagDateRangeFilter.ValueChanged += () =>
			{
				this.TagDateRangeFilterChanged?.Invoke(this.TagDateRangeFilter);
				this.ShowTags();

				this.RaiseNotify($"{nameof(this.TagDateRangeFilter)}.{nameof(this.TagDateRangeFilter.CurrentMinDate)}");
				this.RaiseNotify($"{nameof(this.TagDateRangeFilter)}.{nameof(this.TagDateRangeFilter.CurrentMaxDate)}");
			};

			var mapOverlayZonesWithHomographyAssigned = GetMapOverlayZonesWithHomographyAssigned();

			var earliestDateTasks = new List<Task<HyperDocument[]>>();
			var latestDateTasks = new List<Task<HyperDocument[]>>();

			foreach (var zone in mapOverlayZonesWithHomographyAssigned)
			{
				var metadataSetId = zone.MetadataSetId ?? this.MetadataSetId;
				var metadataSetFilter = await HyperArgsSink.ExecuteAsync(new RetrieveHyperDocumentArgs(metadataSetId.Value));

				var camEnhancementId = zone.FixedCameraEnhancementId.Value;
				var enhancement = (await HyperArgsSink.ExecuteAsync(new RetrieveHyperDocumentArgs(camEnhancementId))).GetPayload<FixedCameraEnhancedData>();
				var fixedCameraEnhancedDataLayer = enhancement.Layers.SingleOrDefault(l => (l as HyperTagFixedCameraEnhancedDataLayer) != null) as HyperTagFixedCameraEnhancedDataLayer;
				var homographyGeometry = fixedCameraEnhancedDataLayer.Tags.SingleOrDefault(t => t.Elements.Any(e => e is HomographyTagElement && (e as HomographyTagElement).Alias == zone.Alias))
					?.Elements.Single(e => e is HyperTagGeometry) as HyperTagGeometry;

				if (metadataSetFilter != null && homographyGeometry != null)
				{

					var metadataSet = metadataSetFilter.GetPayload<HyperMetadataSet>();

					var findArgs = new FindHyperDocumentsArgs(typeof(HyperTag));
					var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(HyperArgsSink, metadataSet);
					findArgs.DescriptorConditions.AddCondition(conditions.Result);

					findArgs.Limit = 1;
					findArgs.OrderByFields = new OrderByField[]
					{
						new OrderByField()
						{
							Ascending = true,
							DescriptorField = true,
							FieldName = "Elements.UniversalTime"
						}
					};

					var docsTask = HyperArgsSink.ExecuteAsync(findArgs).AsTask();
					earliestDateTasks.Add(docsTask);



					var lastTagFindArgs = new FindHyperDocumentsArgs(typeof(HyperTag));
					lastTagFindArgs.DescriptorConditions.AddCondition(conditions.Result);
					lastTagFindArgs.OrderByFields = new OrderByField[]
					{
						new OrderByField()
						{
							Ascending = false,
							DescriptorField = true,
							FieldName = "Elements.UniversalTime"
						}
					};
					lastTagFindArgs.Limit = 1;

					docsTask = HyperArgsSink.ExecuteAsync(lastTagFindArgs).AsTask();
					latestDateTasks.Add(docsTask);


				}
			}
			await Task.WhenAll(earliestDateTasks);
			await Task.WhenAll(latestDateTasks);

			DateTime? latestDate = null;
			DateTime? earliestDate = null;
			for (int zoneIndex = 0; zoneIndex < mapOverlayZonesWithHomographyAssigned.Count; zoneIndex++)
			{
				var docs = earliestDateTasks[zoneIndex].Result;
				var firstTag = docs[0].GetPayload<HyperTag>();
				var currentEarliestDate = (firstTag.Elements.Single(e => e is HyperTagTime && (e as HyperTagTime).TimeType == HyperTagTime.TimeTypes.UniversalTime) as HyperTagTime).UniversalTime;
				if (earliestDate == null || currentEarliestDate < earliestDate)
				{
					earliestDate = currentEarliestDate;
				}

				docs = latestDateTasks[zoneIndex].Result;
				var lastTag = docs[0].GetPayload<HyperTag>();
				var currentLatestDate = (lastTag.Elements.Single(e => e is HyperTagTime && (e as HyperTagTime).TimeType == HyperTagTime.TimeTypes.UniversalTime) as HyperTagTime).UniversalTime;

				if (latestDate == null || currentLatestDate > latestDate)
				{
					latestDate = currentLatestDate;
				}
			}

			if (mapOverlayZonesWithHomographyAssigned.Any())
			{
				TagDateRangeFilter.InitRangeSlider(earliestDate.Value, latestDate.Value, this.TagDateFilterPreInitialized);
				this.RaiseNotify("TagDateRangeFilter");
			}

			this.InitializeAutoplayTagDateRangeFilter();

			this.TagDateRangeFilterChanged?.Invoke(this.TagDateRangeFilter);
			this.TagsAreBeingLoaded.Value = false;
		}

		private void InitializeAutoplayTagDateRangeFilter()
		{
			this.AutoplayTagDateRangeFilter = new TagDateRangeFilterOptions();
			this.AutoplayTagDateRangeFilter.InitRangeSlider(this.TagDateRangeFilter.MinDate, this.TagDateRangeFilter.MaxDate);
		}
		#endregion //Initializing

		#region AutoPlay
		private Timer _playbackTimer;
		public async Task RunPlayback()
		{
			if (this.IsAutoPlayOn)
			{
				return;
			}

			this.IsAutoPlayOn.Value = true;

			// Initialize playback timer
			_playbackTimer = new Timer(this.PlaybackOptions.PlayDuration.TotalMilliseconds);
			_playbackTimer.AutoReset = false;

			var isCacheMode = this.PlaybackOptions.LoadMode == MapPlaybackOptions.LoadModeEnum.Cache;

			// Initalize dates for the first two segments
			var dateRangeFilter = this.AutoplayTagDateRangeFilter;
			var dateSegmentSize = this.PlaybackOptions.PlayStep;
			var currentSegmentMinDate = dateRangeFilter.MinDate;
			var currentSegmentMaxDate = (currentSegmentMinDate + dateSegmentSize) < dateRangeFilter.MaxDate ? (currentSegmentMinDate + dateSegmentSize) : dateRangeFilter.MaxDate;
			var nextSegmentMaxDate = (currentSegmentMaxDate + dateSegmentSize) < dateRangeFilter.MaxDate ? (currentSegmentMaxDate + dateSegmentSize) : dateRangeFilter.MaxDate;

			// Initialize data sets for the first two segments
			List<ZoneDataSet> currentTagSets;
			List<ZoneDataSet> nextTagSets;
			if (isCacheMode)
			{
				currentTagSets = await GetTagsForDateRangeFromCache(currentSegmentMinDate, currentSegmentMaxDate);
				nextTagSets = await GetTagsForDateRangeFromCache(currentSegmentMaxDate, nextSegmentMaxDate);
			}
			else
			{
				currentTagSets = await GetZoneDataSetsForDateRange(currentSegmentMinDate, currentSegmentMaxDate);
				nextTagSets = await GetZoneDataSetsForDateRange(currentSegmentMinDate, nextSegmentMaxDate);
			}

			ZoneDataSets = currentTagSets;

			// Run first step outside the timer
			dateRangeFilter.CurrentMinDate = currentSegmentMinDate;
			dateRangeFilter.CurrentMaxDate = currentSegmentMaxDate;
			await ShowTagsForCurrentTagSets();
			this.RaiseNotify($"{nameof(this.AutoplayTagDateRangeFilter)}.{nameof(this.AutoplayTagDateRangeFilter.CurrentMinDate)}");
			this.RaiseNotify($"{nameof(this.AutoplayTagDateRangeFilter)}.{nameof(this.AutoplayTagDateRangeFilter.CurrentMaxDate)}");

			// Start timer 
			_playbackTimer.Start();
			_playbackTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
			{
				if (!this.IsAutoPlayOn)
				{
					_playbackTimer.Stop();
					return;
				}

				ZoneDataSets = nextTagSets;
				currentSegmentMinDate = currentSegmentMaxDate;
				currentSegmentMaxDate = (currentSegmentMaxDate + dateSegmentSize) < dateRangeFilter.MaxDate ? (currentSegmentMaxDate + dateSegmentSize) : dateRangeFilter.MaxDate;
				nextSegmentMaxDate = (currentSegmentMaxDate + dateSegmentSize) < dateRangeFilter.MaxDate ? (currentSegmentMaxDate + dateSegmentSize) : dateRangeFilter.MaxDate;

				dateRangeFilter.CurrentMinDate = currentSegmentMinDate;
				dateRangeFilter.CurrentMaxDate = currentSegmentMaxDate;
				ShowTagsForCurrentTagSets().Wait();
				this.RaiseNotify($"{nameof(this.AutoplayTagDateRangeFilter)}.{nameof(this.AutoplayTagDateRangeFilter.CurrentMinDate)}");
				this.RaiseNotify($"{nameof(this.AutoplayTagDateRangeFilter)}.{nameof(this.AutoplayTagDateRangeFilter.CurrentMaxDate)}");


				if (currentSegmentMaxDate >= dateRangeFilter.MaxDate)
				{
					StopPlayback();
				}
				else
				{
					if (this.IsVmShowingHeatmapProp)
					{
						if (isCacheMode)
						{
							CurrentlyShownHeatmapZoneDataSet = this.PlaybackCache.Value.Steps
								.Single(s => s.From == currentSegmentMinDate && s.To == currentSegmentMaxDate).ZoneDataSets.Single(ds => ds.Zone.Id == CurrentlyShownHeatmapZoneDataSet.Zone.Id);
						}
						else
						{
							CurrentlyShownHeatmapZoneDataSet = ZoneDataSets.Single(ds => ds.Zone.Id == CurrentlyShownHeatmapZoneDataSet.Zone.Id);
						}

						HeatmapImgProp.Value = CurrentlyShownHeatmapZoneDataSet.Heatmap.DataBase64Link;
					}

					if (isCacheMode)
					{
						nextTagSets = GetTagsForDateRangeFromCache(currentSegmentMaxDate, nextSegmentMaxDate).Result;
					}
					else
					{
						nextTagSets = GetZoneDataSetsForDateRange(currentSegmentMaxDate, nextSegmentMaxDate).Result;
					}
				}

				if (this.IsAutoPlayOn)
				{
					_playbackTimer.Start();
				}
			};
		}

		private async Task PopulateZoneDataSetsWithHeatmaps(List<ZoneDataSet> currentTagSets, Helpers.MasksHeatmapRenderer heatmapRenderer)
		{
			foreach (var ts in currentTagSets)
			{
				if (ts.Zone.FixedCameraEnhancementId.HasValue)
				{
					ts.Heatmap = await heatmapRenderer.GenerateFromTagsAsync(ts.Tags, ts.Zone.FixedCameraEnhancementId.Value, false);
				}
			}
		}

		public async Task StopPlayback()
		{
			if (_playbackTimer != null)
			{
				_playbackTimer.Stop();
				_playbackTimer.Dispose();
				this.IsAutoPlayOn.Value = false;
				
				await ShowTags();
			}
		}

		public async Task RefreshMapDataCache()
		{
			this.PlaybackCacheBeingUpdated = true;

			var mapCache = new MapPlaybackCache();

			var dateSegmentSize = this.PlaybackOptions.PlayStep;
			var stepFromDate = this.TagDateRangeFilter.MinDate;
			var stepToDate = (stepFromDate + dateSegmentSize) < this.TagDateRangeFilter.MaxDate ? (stepFromDate + dateSegmentSize) : this.TagDateRangeFilter.MaxDate;
			mapCache.Steps.Add(new PlaybackStepCache
			{
				From = stepFromDate,
				To = stepToDate
			});


			while (stepToDate != this.TagDateRangeFilter.MaxDate)
			{
				stepFromDate = stepToDate;
				stepToDate = (stepFromDate + dateSegmentSize) < this.TagDateRangeFilter.MaxDate ? (stepFromDate + dateSegmentSize) : this.TagDateRangeFilter.MaxDate;
				mapCache.Steps.Add(new PlaybackStepCache
				{
					From = stepFromDate,
					To = stepToDate
				});
			}

			var stepTasks = new List<Task<List<ZoneDataSet>>>();
			var stepToDatasetMappings = new Dictionary<PlaybackStepCache, Task<List<ZoneDataSet>>>();
			foreach (var step in mapCache.Steps)
			{
				var task = GetZoneDataSetsForDateRange(step.From, step.To);
				stepTasks.Add(task);
				stepToDatasetMappings[step] = task;
			}

			await Task.WhenAll(stepTasks);

			var heatmapRenderer = InstantiateHeatmapRendererWithSettings();
			foreach (var kv in stepToDatasetMappings)
			{
				var zoneDataSets = kv.Key.ZoneDataSets = kv.Value.Result;

				foreach (var ds in zoneDataSets)
				{
					ds.Heatmap = await heatmapRenderer.GenerateFromTagsAsync(ds.Tags, ds.Zone.FixedCameraEnhancementId.Value, false);
				}
			}

			this.PlaybackCache.Value = mapCache;
			await this.OnMapPlayebackCacheUpdated?.Invoke(mapCache);

			this.PlaybackCacheBeingUpdated.Value = false;
		}

		private async Task<List<ZoneDataSet>> GetTagsForDateRangeFromCache(DateTime from, DateTime to)
		{
			var mapOverlayZonesWithHomographyAssigned = GetMapOverlayZonesWithHomographyAssigned();
			var zonesHypZoneHyperTagSets = new List<ZoneDataSet>();

			foreach (var zone in mapOverlayZonesWithHomographyAssigned)
			{
				var zoneDataSet = this.PlaybackCache.Value.Steps.Single(s => s.From == from && s.To == to).ZoneDataSets.Single(ds => ds.Zone.Id == zone.Id);

				zonesHypZoneHyperTagSets.Add(zoneDataSet);
			}

			return zonesHypZoneHyperTagSets;
		}
		#endregion

		public async Task UpdateZone(ZoneOverlayEntryJsModel zoneModel)
		{
			var zone = ZoneDataSets.SingleOrDefault(z => z.Zone.Id == zoneModel.Id)?.Zone;

			// if there is data (tags) loaded for the zone
			if (zone != null)
			{
				zone.Points = zoneModel.Points;

				await UpdateSvgMapZone(zone);
			}
		}

		private async Task<List<ZoneDataSet>> GetZoneDataSetsForDateRange(DateTime from, DateTime to, bool populateWithHeatmaps = false)
		{
			var mapOverlayZonesWithHomographyAssigned = GetMapOverlayZonesWithHomographyAssigned();
			var zonesHypZoneHyperTagSets = new List<ZoneDataSet>();

			foreach (var zone in mapOverlayZonesWithHomographyAssigned)
			{
				var metadataSetId = zone.MetadataSetId ?? this.MetadataSetId;
				var metadataSetFilter = await HyperArgsSink.ExecuteAsync(new RetrieveHyperDocumentArgs(metadataSetId.Value));
				var homographyGeometry = await GetHomographyGeometryByCamEnhancementId(zone.FixedCameraEnhancementId.Value, zone.Alias);
				var metadataSet = metadataSetFilter.GetPayload<HyperMetadataSet>();

				var findArgs = new FindHyperDocumentsArgs(typeof(HyperTag));
				metadataSet.FromDate = from;
				metadataSet.ToDate = to;
				var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(HyperArgsSink, metadataSet);
				findArgs.DescriptorConditions.AddCondition(conditions.Result);
				findArgs.Limit = this.TagRequestMaxCountLimit;

				var docs = await HyperArgsSink.ExecuteAsync(findArgs);
				var hyperTags = new List<HyperTag>();
				foreach (var doc in docs)
				{
					hyperTags.Add(doc.GetPayload<HyperTag>());
				}

				List<HyperTag> zoneTags = FilterTagsByHomographyArea(hyperTags, homographyGeometry, zone);
				var zoneDataSet = new ZoneDataSet { Tags = zoneTags, Zone = zone, HomographyGeometry = homographyGeometry };
				zonesHypZoneHyperTagSets.Add(zoneDataSet);
			}

			if (populateWithHeatmaps)
			{
				using (var heatmapRenderer = InstantiateHeatmapRendererWithSettings())
				{
					await PopulateZoneDataSetsWithHeatmaps(zonesHypZoneHyperTagSets, heatmapRenderer);
				}
			}

			return zonesHypZoneHyperTagSets;
		}

		/// <summary>
		/// Generates instance of Heatmap Renderer filling it's settings with properties from VM
		/// </summary>
		/// <param name="heatmapMode">If we should render heatmap or real images.</param>
		/// <returns></returns>
		private Helpers.MasksHeatmapRenderer InstantiateHeatmapRendererWithSettings(bool heatmapMode = true)
		{
			var settings = new Helpers.MasksHeatmapRenderer.HeatmapSettings
			{
				UseCustomNormalizationSettings = HeatmapCustomNormalization,
				MinimumNumberOfOverlaps = HeatmapNormalizationMinOverlaps,
				MaximumNumberOfOverlaps = HeatmapNormalizationMaxOverlaps,
				RenderingMode = heatmapMode ? _heatmapRendererMode : Helpers.MasksHeatmapRenderer.RenderingMode.RealImage
			};
			var helper = new Helpers.MasksHeatmapRenderer(HyperArgsSink, null, settings);
			return helper;
		}

		public ZoneOverlayEntryJsModel AddNewZoneToVm(JsModel.ZoneOverlayEntryJsModel zone)
		{
			var zoneDomainModel = zone.ToDomainModel();
			this.MapOverlay.Value.Entries.Add(zoneDomainModel);

			return ZoneOverlayEntryJsModel.CreateFromDomainModel(zoneDomainModel);
		}

		private async Task<byte[]> LoadTagImage(HyperTag tag)
		{
			var ids = tag.GetElements<IHyperTagHyperIds>().FirstOrDefault(e => e.HyperId.TrackId.Value.Type == HyperTrackTypes.Video);

			var geometry = tag.GetElement<HyperTagGeometry>();
			var args2 = new RetrieveFragmentFramesArgs()
			{
				AssetId = ids.HyperId.AssetId.Value,
				FragmentId = ids.HyperId.HasFullFragmentData ? ids.HyperId.FragmentId.Value : new HyperFragmentId(0),
				SliceIds = new HyperSliceId[] { ids.HyperId.HasFullSliceData ? ids.HyperId.SliceId.Value : new HyperSliceId(0) },
				GeometryItem = geometry?.GeometryItem
			};

			var sliceResult = await HyperArgsSink.ExecuteAsync<RetrieveFragmentFramesArgs.SliceResult[]>(args2);

			return sliceResult[0].Image.Data;
		}

		private Dictionary<CircleOverlayEntryJsModel, HyperTag> _circlesToTagsMappings = new Dictionary<CircleOverlayEntryJsModel, HyperTag>();
		public async Task ShowTags()
		{
			this.TagsAreBeingLoaded.Value = true;

			if (TagDateRangeFilter != null)
			{
				ZoneDataSets = await GetZoneDataSetsForDateRange(TagDateRangeFilter.CurrentMinDate, TagDateRangeFilter.CurrentMaxDate);
			}

			await ShowTagsForCurrentTagSets();

			this.TagsAreBeingLoaded.Value = false;

			RaiseNotify(nameof(HeatmapAvailableForSelectedZone));
		}

		private async Task ShowTagsForCurrentTagSets()
		{
			var mapOverlayZonesWithHomographyAssigned = GetMapOverlayZonesWithHomographyAssigned();

			var circlesToRemove = _circlesToTagsMappings.Select(kv => new MapOverlayUpdateDetails
			{
				Type = MapOverlayUpdateDetails.DeleteUpdateType,
				OverlayEntry = JsonSerializer.Serialize(kv.Key, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
			}).ToArray();

			await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, circlesToRemove, false, "batch" });

			foreach (var zone in mapOverlayZonesWithHomographyAssigned)
			{
				await UpdateSvgMapZone(zone);
			}
		}

		private async Task<HyperTagGeometry> GetHomographyGeometryByCamEnhancementId(HyperDocumentId camEnhancementId, string zoneAlias)
		{
			var enhancement = (await HyperArgsSink.ExecuteAsync(new RetrieveHyperDocumentArgs(camEnhancementId))).GetPayload<FixedCameraEnhancedData>();
			var fixedCameraEnhancedDataLayer = enhancement.Layers.SingleOrDefault(l => (l as HyperTagFixedCameraEnhancedDataLayer) != null) as HyperTagFixedCameraEnhancedDataLayer;
			var homographyGeometry = fixedCameraEnhancedDataLayer.Tags.SingleOrDefault(t => t.Elements.Any(e => e is HomographyTagElement && (e as HomographyTagElement).Alias == zoneAlias))
				?.Elements.Single(e => e is HyperTagGeometry) as HyperTagGeometry;

			return homographyGeometry;
		}

		private async Task UpdateSvgMapZone(ZoneOverlayEntry zone)
		{
			List<MapOverlayUpdateDetails> updateDetailsBatch = new List<MapOverlayUpdateDetails>();
			var zoneDataSet = ZoneDataSets.Single(ds => ds.Zone.Id == zone.Id);
			var zoneTags = zoneDataSet.Tags;
			var zoneHomographuGeometry = zoneDataSet.HomographyGeometry;

			foreach (var tag in zoneTags)
			{
				var tagGeometry = (tag.Elements.Single(t => t as HyperTagGeometry != null) as HyperTagGeometry).GeometryItem;
				var rect = (UniRectangle2f)tagGeometry.Shape;
				var homographyRect = (UniPolygon2f)zoneHomographuGeometry.GeometryItem.Shape;
				var bottomCenter = tagGeometry.Transformation.Transform(new UniPoint2f(((rect.BottomRight + rect.BottomLeft) / 2).X, rect.BottomLeft.Y));
				var mapperCentrePoint = MapHomographyPoint(bottomCenter, homographyRect.Points, zone.Points);
				var circle = new CircleOverlayEntryJsModel
				{
					Id = Guid.NewGuid().ToString(),
					Center = mapperCentrePoint,
					Size = 2.5,
					IsSelectable = false,
					IsDraggable = false
				};
				circle.EventHandlerMappings.Add("click", "ShowTagInfo");

				_circlesToTagsMappings.Add(circle, tag);

				var circleSerialized = JsonSerializer.Serialize(circle, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

				var updateDetails = new MapOverlayUpdateDetails()
				{
					Type = MapOverlayUpdateDetails.AddOrUpdateUpdateType,
					OverlayEntry = circleSerialized,
				};

				updateDetailsBatch.Add(updateDetails);
			}

			await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, updateDetailsBatch, false, "batch" });
		}

		private List<HyperTag> FilterTagsByHomographyArea(IEnumerable<HyperTag> tags, HyperTagGeometry homographyGeometry, ZoneOverlayEntry zone)
		{
			List<HyperTag> zoneTags = new List<HyperTag>();
			foreach (var tag in tags)
			{
				var tagGeometry = (tag.Elements.SingleOrDefault(t => t as HyperTagGeometry != null) as HyperTagGeometry)?.GeometryItem;
				if (tagGeometry != null)
				{
					var rect = (UniRectangle2f)tagGeometry.Shape;
					var bottomCenter = tagGeometry.Transformation.Transform(new UniPoint2f(((rect.BottomRight + rect.BottomLeft) / 2).X, rect.BottomLeft.Y));
					var homographyRect = (UniPolygon2f)homographyGeometry.GeometryItem.Shape;
					if (UniPolygon2f.IsInside(homographyRect.Points,
						bottomCenter))
					{
						zoneTags.Add(tag);
					}
				}
			}

			return zoneTags;
		}

		public void OpenTagProperties()
		{
			this.ShowingHyperTagProperties.Value = true;
		}

		public async Task ShowTagInfo(CircleOverlayEntryJsModel circle, double pageX, double pageY)
		{
			this.ShowingHyperTagInfo.Value = false;

			var tagToShow = _circlesToTagsMappings.Single(kv => kv.Key.Id == circle.Id).Value;
			this.CurrentTagBeingShown.Value = tagToShow;

			this.TagInfoImage = await LoadTagImage(tagToShow);

			this.HyperTagInfoXPos = pageX - 100;
			this.HyperTagInfoYPos = pageY - 100;
			this.ShowingHyperTagInfo.Value = true;
		}

		private UniPoint2f MapHomographyPoint(UniPoint2f bottomCenter, UniPoint2f[] pointsSrc, UniPoint2f[] pointsDst)
		{
			var cvSrcPoints = pointsSrc.Select(p => new OpenCvSharp.Point2d(p.X, p.Y));
			var cvDstPoints = pointsDst.Select(p => new OpenCvSharp.Point2d(p.X, p.Y));

			OpenCvSharp.Mat hCv = OpenCvSharp.Cv2.FindHomography(cvSrcPoints, cvDstPoints);

			OpenCvSharp.Point2f resultPoint = OpenCvSharp.Cv2.PerspectiveTransform(new OpenCvSharp.Point2f[] { new OpenCvSharp.Point2f(bottomCenter.X, bottomCenter.Y) }, hCv)[0];

			return new UniPoint2f(resultPoint.X, resultPoint.Y);
		}

		public async Task SaveMapOverlay(MapOverlay overlay)
		{
			// Update map overlay state
			//foreach (var entry in overlay.Entries)
			//{
			//	var oldStateEntry = MapOverlay.Value.Entries.SingleOrDefault(e => e.Id == entry.Id);

			//	// Add new entries
			//	if (oldStateEntry == null) 
			//	{
			//		MapOverlay.Value.Entries.Add(entry);
			//	}
			//	// Update existed before
			//	else
			//	{
			//		oldStateEntry.UpdateFrom(entry);
			//	}
			//}

			//// remove deleted entries
			//var entriesToDelete = new List<OverlayEntry>();
			//foreach (var entry in this.MapOverlay.Value.Entries) 
			//{
			//	if(overlay.Entries.Any(e => e.Id == entry.Id) == false)
			//	{
			//		entriesToDelete.Add(entry);
			//	}
			//}
			//this.MapOverlay.Value.Entries.RemoveAll(e => entriesToDelete.Contains(e));
			this.MapOverlay.Value = overlay;

			// Save Hyper doc
			var doc = new HyperDocument(MapOverlay.Value);
			var storeDocArgs = new StoreHyperDocumentArgs(doc);
			await this.HyperArgsSink.ExecuteAsync(storeDocArgs);
		}

		public void SelectZone(ZoneOverlayEntryJsModel zone)
		{
			this.CurrentlySelectedZoneId = zone.Id;
			
			RaiseNotify(nameof(HeatmapAvailableForSelectedZone));
		}

		public void UnselectZone(ZoneOverlayEntryJsModel zone)
		{
			this.CurrentlySelectedZoneId = null;

			RaiseNotify(nameof(HeatmapAvailableForSelectedZone));
		}

		public void OpenSvgControlProps(string id)
		{
			var currentPropGridObj = this.MapOverlay.Value.Entries.Single(e => e.Id == id);
			this.CurrentPropertyGridObject = currentPropGridObj;

			this.ShowingControlPropertyGrid.Value = true;
		}

		public async Task UpdateSelectedControlProperties()
		{
			var jsModel = GetJsModelForOverlayEntry(this.CurrentPropertyGridObject);

			var progGridObjectSerialized = JsonSerializer.Serialize<object>(jsModel, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

			var updateDetails = new MapOverlayUpdateDetails()
			{
				Type = MapOverlayUpdateDetails.AddOrUpdateUpdateType,
				OverlayEntry = progGridObjectSerialized
			};

			await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, updateDetails, true, "single" });

			this.ShowingControlPropertyGrid.Value = false;
		}

		private object GetJsModelForOverlayEntry(OverlayEntry entry)
		{
			var entryType = entry.GetType();

			object model = null;

			if (entryType == typeof(ZoneOverlayEntry))
			{
				model = ZoneOverlayEntryJsModel.CreateFromDomainModel(entry as ZoneOverlayEntry);
			}

			if (entryType == typeof(CircleOverlayEntry))
			{
				model = CircleOverlayEntryJsModel.CreateFromDomainModel(entry as CircleOverlayEntry);
			}

			if (entryType == typeof(CameraOverlayEntry))
			{
				model = CameraOverlayEntryJsModel.CreateFromDomainModel(entry as CameraOverlayEntry);
			}

			return model;
		}

		private List<ZoneOverlayEntry> GetMapOverlayZonesWithHomographyAssigned()
		{
			var zones = this.MapOverlay.Value.Entries
				.Where(e => e.GetType() == typeof(ZoneOverlayEntry))
				.Cast<ZoneOverlayEntry>()
				.Where(z => z.FixedCameraEnhancementId != null && !string.IsNullOrWhiteSpace(z.Alias))
				.Where(z => z.MetadataSetId != null || this.MetadataSetId != null)
				.ToList();

			return zones;
		}

		public override void Dispose()
		{
			if (_playbackTimer != null)
			{
				this._playbackTimer.Stop();
				this._playbackTimer.Close();
			}

			if (_heatmapRenderer != null)
			{
				_heatmapRenderer.Dispose();
			}

			JsRuntime.InvokeVoidAsync("window.Orions.SvgMapEditor.destroy", new object[] { this._componentContainerId });
		}
		#endregion // Methods

		#region Inner classes

		public enum HeatmapRenderingMode
		{
			Masks,
			LowerPointOfGeometry
		}

		public class TagDateRangeFilterOptions
		{
			public double[] Value { get; set; }
			public double Step { get; set; }
			public double MaxRangeValue { get; set; }
			public DateTime MinDate { get; set; }
			public DateTime MaxDate { get; set; }

			private DateTime _currentMinDate;
			public DateTime CurrentMinDate
			{
				get
				{
					return _currentMinDate;
				}
				set
				{
					Value = new double[] { (double)((value - MinDate).TotalSeconds), (double)((CurrentMaxDate - MinDate).TotalSeconds) };
					_currentMinDate = value;
				}
			}

			private DateTime _currentMaxDate;
			public DateTime CurrentMaxDate
			{
				get
				{
					return _currentMaxDate;
				}
				set
				{
					Value = new double[] { (double)((CurrentMinDate - MinDate).TotalSeconds), (double)((value - MinDate).TotalSeconds) };
					_currentMaxDate = value;
				}
			}

			public event Action ValueChanged;

			private Timer _dateFilterChangeThrottlingTimer;

			public void InitRangeSlider(DateTime minDate, DateTime maxDate, bool keepCurrentValues = false)
			{
				MinDate = minDate;
				MaxDate = maxDate;
				MaxRangeValue = (MaxDate - MinDate).TotalSeconds;

				if (keepCurrentValues)
				{
					if (CurrentMinDate < MinDate)
					{
						CurrentMinDate = MinDate;
					}
					if (CurrentMaxDate > MaxDate)
					{
						CurrentMaxDate = MaxDate;
					}
				}
				else
				{
					CurrentMinDate = MinDate;
					CurrentMaxDate = MinDate.AddDays(1);
					Step = 10;
				}

				//Value = new double[] { (double)((CurrentMinDate - MinDate).TotalSeconds), (double)((CurrentMaxDate - MinDate).TotalSeconds) };
			}

			private object _dateFilterLock = new object();
			public void SliderValueChanged(double[] value)
			{
				ElapsedEventHandler Timer_Elapsed = delegate (object sender, ElapsedEventArgs e)
				{
					_dateFilterChangeThrottlingTimer.Dispose();
					_dateFilterChangeThrottlingTimer = null;

					this.ValueChanged?.Invoke();
				};

				lock (_dateFilterLock)
				{
					if (_dateFilterChangeThrottlingTimer == null)
					{
						_dateFilterChangeThrottlingTimer = new System.Timers.Timer(2000);
						_dateFilterChangeThrottlingTimer.Elapsed += Timer_Elapsed;
						_dateFilterChangeThrottlingTimer.AutoReset = false;
						_dateFilterChangeThrottlingTimer.Start();
					}
					else
					{
						_dateFilterChangeThrottlingTimer.Stop();
						_dateFilterChangeThrottlingTimer.Start();
					}

					var min = MinDate.AddSeconds((MaxDate - MinDate).TotalSeconds * (value[0] / MaxRangeValue));
					var max = MinDate.AddSeconds((MaxDate - MinDate).TotalSeconds * (value[1] / MaxRangeValue));

					CurrentMinDate = min;
					CurrentMaxDate = max;
					Value = value;
				}

			}
		}

		public class MapPlaybackOptions
		{
			public TimeSpan PlayStep { get; set; }
			public TimeSpan PlayDuration { get; set; }
			public LoadModeEnum LoadMode { get; set; }

			public enum LoadModeEnum
			{
				Live = 0,
				Cache = 1
			}
		}

		#endregion // Inner classes
	}

	public class MapOverlayUpdateDetails
	{
		public const string AddOrUpdateUpdateType = "addOrUpdate";
		public const string DeleteUpdateType = "delete";

		public string Type { get; set; }

		public string OverlayEntry { get; set; }
	}

	public class SvgEditorConfig
	{
		public string ZoneColor { get; set; }
		public string CameraColor { get; set; }
		public string CircleColor { get; set; }
		public bool IsReadOnly { get; set; }
	}
}
