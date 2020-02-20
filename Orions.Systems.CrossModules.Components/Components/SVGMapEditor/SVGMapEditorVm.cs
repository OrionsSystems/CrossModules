using System;
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
		private Helpers.MasksHeatmapRenderer _renderer;

		#region Properties
		public IHyperArgsSink HyperArgsSink { get; set; }
		public IJSRuntime JsRuntime { get; set; }
		public HyperDocumentId? MapOverlayId { get; set; }
		public HyperDocumentId? MetadataSetId { get; set; }
		public ViewModelProperty<MapOverlay> MapOverlay { get; set; } = new ViewModelProperty<MapOverlay>(new Infrastructure.HyperMedia.MapOverlay.MapOverlay());
		public Func<HyperDocumentId?, Task> OnMapOverlayIdSet { get; set; }
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
		public ViewModelProperty<bool> HomographiesDetected { get; set; } = false;
		public ViewModelProperty<bool> IsAutoPlayOn { get; set; } = false;
		#endregion // Properties

		private List<ZoneDataSet> ZoneDataSets = new List<ZoneDataSet>();

		private string _componentContainerId;

		public SVGMapEditorVm()
		{
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

		private async Task OpenPopupMap(string zoneId, bool heatmapMode)
		{
			var zoneOverlayEntry = this.MapOverlay.Value.Entries.Single(z => z.Id == zoneId);

			if (ZoneDataSets.Any(ds => ds.Zone.Id == zoneId))
			{
				var zoneDataSet = ZoneDataSets.Single(z => z.Zone.Id == zoneId);
				this.CurrentlyShownHeatmapZoneDataSet = zoneDataSet;

				var tagsForMap = zoneDataSet.Tags;
				var metadataSetId = zoneDataSet.Zone.MetadataSetId;
				var fixedCameraEnhancementId = zoneDataSet.Zone.FixedCameraEnhancementId;

				if (metadataSetId.HasValue && fixedCameraEnhancementId.HasValue && tagsForMap != null && tagsForMap.Any())
				{
					IsVmShowingHeatmapProp.Value = true;
					if (this.IsAutoPlayOn && zoneDataSet.Heatmap != null)
					{
						this.HeatmapImgProp.Value = $"data:image/jpg;base64, {Convert.ToBase64String(zoneDataSet.Heatmap.Data)}" ;
					}
					else
					{
						_renderer = new Helpers.MasksHeatmapRenderer(HyperArgsSink, null, new Helpers.MasksHeatmapRenderer.HeatmapSettings());

						PrepareHeatmap(tagsForMap, fixedCameraEnhancementId.Value, heatmapMode);
					}
				}
			}
		}

		private async Task PrepareHeatmap(List<HyperTag> tagsForMap, HyperDocumentId fixedCameraEnhancementId, bool heatmapMode)
		{
			var img = await _renderer.GenerateFromTagsAsync(tagsForMap, fixedCameraEnhancementId, heatmapMode, false);

			if (img != null)
			{
				HeatmapImgProp.Value = $"data:image/jpg;base64, {Convert.ToBase64String(img.Data)}";
			}
		}

		public void CloseHeatmap()
		{
			IsVmShowingHeatmapProp.Value = false;
			HeatmapImgProp.Value = null;
			_renderer?.Dispose();
		}
		#endregion // Heatmap

		#region Initializing
		public async Task Initialize(string componentContainerId, DotNetObjectReference<SVGMapEditorBase> thisReference)
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

		private async Task InitializeTagFilter()
		{
			this.TagsAreBeingLoaded.Value = this.TagDateFilterPreInitialized;

			this.TagDateRangeFilter.ValueChanged += () =>
			{
				if (!this.IsAutoPlayOn)
				{
					this.TagDateRangeFilterChanged?.Invoke(this.TagDateRangeFilter);
					this.ShowTags();

					this.RaiseNotify($"{nameof(this.TagDateRangeFilter)}.{nameof(this.TagDateRangeFilter.CurrentMinDate)}");
					this.RaiseNotify($"{nameof(this.TagDateRangeFilter)}.{nameof(this.TagDateRangeFilter.CurrentMaxDate)}");
				}
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

			this.TagDateRangeFilterChanged?.Invoke(this.TagDateRangeFilter);
			this.TagsAreBeingLoaded.Value = true;
		}
		#endregion //Initializing

		#region AutoPlay
		private Timer _playbackTimer;
		private object _playbackLock = new object();
		public async Task RunPlayback()
		{
			if (this.IsAutoPlayOn)
			{
				return;
			}

			this.IsAutoPlayOn.Value = true;

			_playbackTimer = new Timer(7000);
			_playbackTimer.AutoReset = true;

			var dateSegmentSize = (this.TagDateRangeFilter.MaxDate - this.TagDateRangeFilter.MinDate) / 10;
			var currentMinDate = this.TagDateRangeFilter.MinDate;
			var currentMaxDate = this.TagDateRangeFilter.MinDate + dateSegmentSize;
			var nextMaxDate = currentMaxDate + dateSegmentSize;

			var currentTagSets = await GetTagsForDateRange(currentMinDate, currentMaxDate);
			var heatmapRenderer = new Helpers.MasksHeatmapRenderer(HyperArgsSink, null, new Helpers.MasksHeatmapRenderer.HeatmapSettings());
			foreach (var ts in currentTagSets) 
			{
				if (ts.Zone.FixedCameraEnhancementId.HasValue)
				{
					ts.Heatmap = await heatmapRenderer.GenerateFromTagsAsync(ts.Tags, ts.Zone.FixedCameraEnhancementId.Value, true, false);
				}
			}
			var nextTagSets = await GetTagsForDateRange(currentMaxDate, nextMaxDate);
			foreach (var ts in nextTagSets)
			{
				if (ts.Zone.FixedCameraEnhancementId.HasValue)
				{
					ts.Heatmap = await heatmapRenderer.GenerateFromTagsAsync(ts.Tags, ts.Zone.FixedCameraEnhancementId.Value, true, false);
				}
			}

			ZoneDataSets = currentTagSets;

			this.TagDateRangeFilter.CurrentMinDate = currentMinDate;
			this.TagDateRangeFilter.CurrentMaxDate = currentMaxDate;

			this.RaiseNotify($"{nameof(this.TagDateRangeFilter)}.{nameof(this.TagDateRangeFilter.CurrentMinDate)}");
			this.RaiseNotify($"{nameof(this.TagDateRangeFilter)}.{nameof(this.TagDateRangeFilter.CurrentMaxDate)}");

			ShowTagsForCurrentTagSets();
			_playbackTimer.Start();
			_playbackTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
			{
				lock (_playbackLock)
				{
					ZoneDataSets = nextTagSets;

					ShowTagsForCurrentTagSets();

					if (currentMaxDate >= this.TagDateRangeFilter.MaxDate)
					{
						StopPlayback();
					}
					else
					{
						currentMinDate = currentMaxDate;
						currentMaxDate = currentMaxDate + dateSegmentSize;
						nextMaxDate = currentMaxDate + dateSegmentSize;
						
						this.TagDateRangeFilter.CurrentMinDate = currentMinDate;
						this.TagDateRangeFilter.CurrentMaxDate = currentMaxDate;

						if (this.IsVmShowingHeatmapProp)
						{
							CurrentlyShownHeatmapZoneDataSet = ZoneDataSets.Single(ds => ds.Zone.Id == CurrentlyShownHeatmapZoneDataSet.Zone.Id);
							HeatmapImgProp.Value = $"data:image/jpg;base64, {Convert.ToBase64String(CurrentlyShownHeatmapZoneDataSet.Heatmap.Data)}";
						}

						this.RaiseNotify($"{nameof(this.TagDateRangeFilter)}.{nameof(this.TagDateRangeFilter.CurrentMinDate)}");
						this.RaiseNotify($"{nameof(this.TagDateRangeFilter)}.{nameof(this.TagDateRangeFilter.CurrentMaxDate)}");
						
						nextTagSets = GetTagsForDateRange(currentMaxDate, nextMaxDate).Result;
						foreach (var ts in nextTagSets)
						{
							if (ts.Zone.FixedCameraEnhancementId.HasValue)
							{
								ts.Heatmap = heatmapRenderer.GenerateFromTagsAsync(ts.Tags, ts.Zone.FixedCameraEnhancementId.Value, true, false).Result;
							}
						}
					}
				}
			};
		}

		public async Task StopPlayback()
		{
			if (_playbackTimer != null && _playbackTimer.Enabled)
			{
				_playbackTimer.Stop();
				this.IsAutoPlayOn.Value = false;
			}
		}
		#endregion

		private async Task<List<ZoneDataSet>> GetTagsForDateRange(DateTime from, DateTime to)
		{
			var mapOverlayZonesWithHomographyAssigned = GetMapOverlayZonesWithHomographyAssigned();
			var zonesHypZoneHyperTagSets = new List<ZoneDataSet>();

			foreach (var zone in mapOverlayZonesWithHomographyAssigned)
			{
				var metadataSetId = zone.MetadataSetId ?? this.MetadataSetId;
				var metadataSetFilter = await HyperArgsSink.ExecuteAsync(new RetrieveHyperDocumentArgs(metadataSetId.Value));

				var homographyGeometry = await GetHomographyGeometryByCamEnhancementId(zone.FixedCameraEnhancementId.Value, zone.Alias);

				if (metadataSetFilter != null && homographyGeometry != null)
				{
					var metadataSet = metadataSetFilter.GetPayload<HyperMetadataSet>();

					var findArgs = new FindHyperDocumentsArgs(typeof(HyperTag));

					metadataSet.FromDate = from;
					metadataSet.ToDate = to;

					var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(HyperArgsSink, metadataSet);
					findArgs.DescriptorConditions.AddCondition(conditions.Result);
					//findArgs.Limit = int.MaxValue;
					findArgs.Limit = this.TagRequestMaxCountLimit;


					var docs = await HyperArgsSink.ExecuteAsync(findArgs);
					var hyperTags = new List<HyperTag>();
					foreach (var doc in docs)
					{
						hyperTags.Add(doc.GetPayload<HyperTag>());
					}

					List<HyperTag> zoneTags = FilterTagsByHomographyArea(hyperTags, homographyGeometry, zone);

					zonesHypZoneHyperTagSets.Add(new ZoneDataSet { Tags = zoneTags, Zone = zone });
				}
			}

			return zonesHypZoneHyperTagSets;
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

			ZoneDataSets = await GetTagsForDateRange(TagDateRangeFilter.CurrentMinDate, TagDateRangeFilter.CurrentMaxDate);

			await ShowTagsForCurrentTagSets();

			this.TagsAreBeingLoaded.Value = false;
		}

		private async Task ShowTagsForCurrentTagSets()
		{
			var mapOverlayZonesWithHomographyAssigned = GetMapOverlayZonesWithHomographyAssigned();

			var circlesToRemove = _circlesToTagsMappings.Select(kv => new MapOverlayUpdateDetails
			{
				Type = MapOverlayUpdateDetails.DeleteUpdateType,
				OverlayEntry = JsonSerializer.Serialize(kv.Key, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
			});

			await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, circlesToRemove, false, "batch" });

			foreach (var zone in mapOverlayZonesWithHomographyAssigned)
			{
				var homographyGeometry = await GetHomographyGeometryByCamEnhancementId(zone.FixedCameraEnhancementId.Value, zone.Alias);
				await UpdateSvgMapZone(zone, homographyGeometry);
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

		private async Task UpdateSvgMapZone(ZoneOverlayEntry zone, HyperTagGeometry homographyGeometry)
		{
			List<MapOverlayUpdateDetails> updateDetailsBatch = new List<MapOverlayUpdateDetails>();
			var zoneTags = ZoneDataSets.Single(ds => ds.Zone.Id == zone.Id).Tags;

			foreach (var tag in zoneTags)
			{
				var tagGeometry = (tag.Elements.Single(t => t as HyperTagGeometry != null) as HyperTagGeometry).GeometryItem;
				var rect = (UniRectangle2f)tagGeometry.Shape;
				var homographyRect = (UniPolygon2f)homographyGeometry.GeometryItem.Shape;
				var bottomCenter = tagGeometry.Transformation.Transform(new UniPoint2f(((rect.BottomRight + rect.BottomLeft) / 2).X, rect.BottomLeft.Y));
				var mapperCentrePoint = MapHomographyPoint(bottomCenter, homographyRect.Points, zone.Points);
				var circle = new CircleOverlayEntryJsModel
				{
					Id = Guid.NewGuid().ToString(),
					Center = mapperCentrePoint,
					Size = 4,
					IsSelectable = false
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
				var tagGeometry = (tag.Elements.Single(t => t as HyperTagGeometry != null) as HyperTagGeometry).GeometryItem;

				var rect = (UniRectangle2f)tagGeometry.Shape;
				var bottomCenter = tagGeometry.Transformation.Transform(new UniPoint2f(((rect.BottomRight + rect.BottomLeft) / 2).X, rect.BottomLeft.Y));
				var homographyRect = (UniPolygon2f)homographyGeometry.GeometryItem.Shape;
				if (UniPolygon2f.IsInside(homographyRect.Points,
					bottomCenter))
				{
					zoneTags.Add(tag);
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
		#endregion // Methods

		#region Inner classes

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
		public class ZoneDataSet
		{
			public ZoneOverlayEntry Zone { get; set; }
			public List<HyperTag> Tags { get; set; }
			public UniImage Heatmap { get; set; }
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
