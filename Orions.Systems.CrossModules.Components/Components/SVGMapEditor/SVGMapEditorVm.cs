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

		public ViewModelProperty<bool> ShowingControlPropertyGrid { get; set; } = false;
		public OverlayEntry CurrentPropertyGridObject { get; set; }
		public ViewModelProperty<bool> ShowingHyperTagInfo { get; set; } = false;
		public ViewModelProperty<HyperTag> CurrentTagBeingShown { get; set; } = new ViewModelProperty<HyperTag>();
		public double HyperTagInfoXPos { get; set; }
		public double HyperTagInfoYPos { get; set; }
		public ViewModelProperty<bool> ShowingHyperTagProperties { get; set; } = false;

		public ViewModelProperty<bool> IsVmShowingHeatmapProp { get; set; } = new ViewModelProperty<bool>(false);

		public ViewModelProperty<string> HeatmapImgProp { get; set; } = new ViewModelProperty<string>();

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

		public ViewModelProperty<bool> EnableFilterControl { get; set; } = new ViewModelProperty<bool>(false);

		public bool TagDateFilterInitialized { get; set; } = false;
		public TagDateRangeFilterOptions TagDateRangeFilter { get; set; } = new TagDateRangeFilterOptions();
		public ViewModelProperty<bool> HomographiesDetected { get; set; } = false;
		#endregion // Properties

		private Dictionary<ZoneOverlayEntry, List<HyperTag>> ZoneHyperTagSets = new Dictionary<ZoneOverlayEntry, List<HyperTag>>();

		private string _componentContainerId;

		public SVGMapEditorVm()
		{
		}

		public async Task OpenHeatmapAsync(string zoneId)
		{
			await OpenPopupMap(zoneId, true);
		}

		public async Task OpenRealMasksMapAsync(string zoneId)
		{
			await OpenPopupMap(zoneId, false);
		}

		private async Task OpenPopupMap(string zoneId, bool heatmapMode)
		{
			var zoneOverlayEntry = this.MapOverlay.Value.Entries.Single(z => z.Id == zoneId);

			if (ZoneHyperTagSets.Any(kv => kv.Key.Id == zoneId))
			{
				var zoneSetKv = this.ZoneHyperTagSets.SingleOrDefault(kv => kv.Key.Id == zoneId);
				var zone = zoneSetKv.Key;

				var tagsForMap = zoneSetKv.Value;
				var metadataSetId = zone.MetadataSetId;
				var fixedCameraEnhancementId = zone.FixedCameraEnhancementId;

				if (metadataSetId.HasValue && fixedCameraEnhancementId.HasValue && tagsForMap != null && tagsForMap.Any())
				{
					_renderer = new Helpers.MasksHeatmapRenderer(HyperArgsSink, null, new Helpers.MasksHeatmapRenderer.HeatmapSettings());
					IsVmShowingHeatmapProp.Value = true;

					PrepareHeatmap(tagsForMap, fixedCameraEnhancementId.Value, heatmapMode);
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
				if (this.TagDateFilterInitialized)
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
			this.EnableFilterControl.Value = this.TagDateFilterInitialized;
			//this.EnableFilterControl.Value = false;

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
				TagDateRangeFilter.InitRangeSlider(earliestDate.Value, latestDate.Value, this.TagDateFilterInitialized);
				this.RaiseNotify("TagDateRangeFilter");
			}

			this.TagDateRangeFilterChanged?.Invoke(this.TagDateRangeFilter);
			this.EnableFilterControl.Value = true;
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
			this.EnableFilterControl.Value = false;

			var mapOverlayZonesWithHomographyAssigned = GetMapOverlayZonesWithHomographyAssigned();

			var circlesToRemove = _circlesToTagsMappings.Select(kv => new MapOverlayUpdateDetails
			{
				Type = MapOverlayUpdateDetails.DeleteUpdateType,
				OverlayEntry = JsonSerializer.Serialize(kv.Key, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
			}).ToArray();

			await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, circlesToRemove, false, "batch" });


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

					metadataSet.FromDate = TagDateRangeFilter.CurrentMinDate;
					metadataSet.ToDate = TagDateRangeFilter.CurrentMaxDate;

					var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(HyperArgsSink, metadataSet);
					findArgs.DescriptorConditions.AddCondition(conditions.Result);
					//findArgs.Limit = int.MaxValue;
					findArgs.Limit = 1000;


					var docs = await HyperArgsSink.ExecuteAsync(findArgs);
					var hyperTags = new List<HyperTag>();
					foreach (var doc in docs)
					{
						hyperTags.Add(doc.GetPayload<HyperTag>());
					}

					List<MapOverlayUpdateDetails> updateDetailsBatch = new List<MapOverlayUpdateDetails>();
					List<HyperTag> zoneTags = new List<HyperTag>();

					foreach (var tag in hyperTags)
					{
						var tagGeometry = (tag.Elements.Single(t => t as HyperTagGeometry != null) as HyperTagGeometry).GeometryItem;

						var rect = (UniRectangle2f)tagGeometry.Shape;
						var bottomCenter = tagGeometry.Transformation.Transform(new UniPoint2f(((rect.BottomRight + rect.BottomLeft) / 2).X, rect.BottomLeft.Y));
						var homographyRect = (UniPolygon2f)homographyGeometry.GeometryItem.Shape;
						if (UniPolygon2f.IsInside(homographyRect.Points,
							bottomCenter))
						{
							zoneTags.Add(tag);

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
					}

					ZoneHyperTagSets[zone] = zoneTags;

					await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, updateDetailsBatch, false, "batch" });
				}

				this.EnableFilterControl.Value = true;
			}
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
			MapOverlay.Value = overlay;

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

		public class TagDateRangeFilterOptions
		{
			public double[] Value { get; set; }
			public double Step { get; set; }
			public double MaxRangeValue { get; set; }
			public DateTime MinDate { get; set; }
			public DateTime MaxDate { get; set; }
			public DateTime CurrentMinDate { get; set; }
			public DateTime CurrentMaxDate { get; set; }

			public event Action ValueChanged;

			private Timer _dateFilterChangeThrottlingTimer;

			private double[] updatedDateFilterdSliderValue;

			public void InitRangeSlider(DateTime minDate, DateTime maxDate, bool keepCurrentValues = false)
			{
				MinDate = minDate;
				MaxDate = maxDate;
				MaxRangeValue = (MaxDate - MinDate).TotalSeconds;

				if (keepCurrentValues)
				{
					if(CurrentMinDate < MinDate)
					{
						CurrentMinDate = MinDate;
					}
					if(CurrentMaxDate > MaxDate)
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

				Value = new double[] { (double)((CurrentMinDate - MinDate).TotalSeconds), (double)((CurrentMaxDate - MinDate).TotalSeconds) };
			}

			private static object _dateFilterLock = new object();
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
