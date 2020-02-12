using Orions.Common;
using System.Collections.Generic;
using Orions.Infrastructure.HyperMedia.MapOverlay;
using System.Threading.Tasks;
using Orions.Node.Common;
using System;
using Orions.Infrastructure.HyperMedia;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Linq;
using Orions.Infrastructure.HyperSemantic;
using Orions.Systems.CrossModules.Components.Components.SVGMapEditor.JsModel;

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor
{
	public class SVGMapEditorVm : BlazorVm
	{
		public IHyperArgsSink HyperArgsSink { get; set; }
		public IJSRuntime JsRuntime { get; set; }
		public HyperDocumentId? MapOverlayId { get; set; }
		public HyperDocumentId? MetadataSetId { get; set; }
		public ViewModelProperty<MapOverlay> MapOverlay { get; set; } = new ViewModelProperty<MapOverlay>(new Infrastructure.HyperMedia.MapOverlay.MapOverlay());
		public Func<HyperDocumentId?, Task> OnMapOverlayIdSet { get; set; }

		public string DefaultCircleColor { get; internal set; }
		public string DefaultZoneColor { get; internal set; }
		public string DefaultCameraColor { get; internal set; }
		public bool IsReadOnly { get; set; }

		public ViewModelProperty<bool> ShowingControlPropertyGrid { get; set; } = false;
		public OverlayEntry CurrentPropertyGridObject { get; set; }


		private string _componentContainerId;


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

			await ShowTags(200);
		}


		//private Dictionary<HyperDocumentId, HyperDocument> _metadataSetCache = new Dictionary<HyperDocumentId, HyperDocument>();
		public async Task ShowTags(int tagCount)
		{
			var mapOverlayZonesWithHomographyAssigned = this.MapOverlay.Value.Entries
				.Where(e => e.GetType() == typeof(ZoneOverlayEntry))
				.Cast<ZoneOverlayEntry>()
				.Where(z => z.FixedCameraEnhancementId != null && !string.IsNullOrWhiteSpace(z.Alias))
				.Where(z => this.MetadataSetId.HasValue ? true : z.MetadataSetId != null);


			foreach (var zone in mapOverlayZonesWithHomographyAssigned)
			{
				var metadataSetId = zone.MetadataSetId ?? this.MetadataSetId;
				var metadataSetFilter = await HyperArgsSink.ExecuteAsync(new RetrieveHyperDocumentArgs(metadataSetId.Value));

				var camEnhancementId = zone.FixedCameraEnhancementId.Value;
				var enhancement = (await HyperArgsSink.ExecuteAsync(new RetrieveHyperDocumentArgs(camEnhancementId))).GetPayload<FixedCameraEnhancedData>();
				var fixedCameraEnhancedDataLayer = enhancement.Layers.SingleOrDefault(l => (l as HyperTagFixedCameraEnhancedDataLayer) != null) as HyperTagFixedCameraEnhancedDataLayer;
				var homographyGeometry = fixedCameraEnhancedDataLayer.Tags.SingleOrDefault(t => t.Elements.Any(e => e is HomographyTagElement && (e as HomographyTagElement).Alias == zone.Alias))
					?.Elements.Single(e => e is HyperTagGeometry) as HyperTagGeometry;
				//var homographyGeometry = fixedCameraEnhancedDataLayer.Tags[0].Elements.Single(t => t as HyperTagGeometry != null) as HyperTagGeometry;

				if (metadataSetFilter != null && homographyGeometry != null)
				{
					var mapZone = this.MapOverlay.Value.Entries.First(z => z as ZoneOverlayEntry != null) as ZoneOverlayEntry;
					var metadataSet = metadataSetFilter.GetPayload<HyperMetadataSet>();

					var findArgs = new FindHyperDocumentsArgs(typeof(HyperTag));

					var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(HyperArgsSink, metadataSet);
					findArgs.DescriptorConditions.AddCondition(conditions);

					findArgs.Limit = tagCount;

					var docs = await HyperArgsSink.ExecuteAsync(findArgs);
					var hyperTags = new List<HyperTag>();
					foreach (var doc in docs)
					{
						hyperTags.Add(doc.GetPayload<HyperTag>());
					}

					foreach (var tag in hyperTags)
					{
						var tagGeometry = (tag.Elements.Single(t => t as HyperTagGeometry != null) as HyperTagGeometry).GeometryItem;

						var rect = (UniRectangle2f)tagGeometry.Shape;
						var bottomCenter = tagGeometry.Transformation.Transform(new UniPoint2f(((rect.BottomRight + rect.BottomLeft) / 2).X, rect.BottomLeft.Y));
						var homographyRect = (UniPolygon2f)homographyGeometry.GeometryItem.Shape;
						if (UniPolygon2f.IsInside(homographyRect.Points,
							bottomCenter))
						{

							var mapperCentrePoint = MapHomographyPoint(bottomCenter, homographyRect.Points, mapZone.Points);
							var circle = new CircleOverlayEntryJsModel
							{
								Center = mapperCentrePoint,
								Size = 4
							};
							var circleSerialized = JsonSerializer.Serialize(circle, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

							var updateDetails = new MapOverlayUpdateDetails()
							{
								Type = MapOverlayUpdateDetails.AddOrUpdateUpdateType,
								OverlayEntry = circleSerialized,
							};

							await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, updateDetails, false });
						}
					}
				}
			}
		}

		private UniPoint2f MapHomographyPoint(UniPoint2f bottomCenter, UniPoint2f[] pointsSrc, UniPoint2f[] pointsDst)
		{
			var cvSrcPoints = pointsSrc.Select(p => new OpenCvSharp.Point2d(p.X, p.Y));
			var cvDstPoints = pointsDst.Select(p => new OpenCvSharp.Point2d(p.X, p.Y));

			OpenCvSharp.Mat hCv = OpenCvSharp.Cv2.FindHomography(cvSrcPoints, cvDstPoints);

			OpenCvSharp.Point2f resultPoint = OpenCvSharp.Cv2.PerspectiveTransform(new OpenCvSharp.Point2f[] { new OpenCvSharp.Point2f(bottomCenter.X, bottomCenter.Y) }, hCv)[0];

			return new UniPoint2f(resultPoint.X, resultPoint.Y);
		}

		public async Task TestLiveUpdate()
		{
			var testZone = this.MapOverlay.Value.Entries.First(e => e as ZoneOverlayEntry != null) as ZoneOverlayEntry;

			var testCircles = new List<CircleOverlayEntryJsModel>();
			var speedCoeffs = new List<float>();
			Random rnd = new Random();

			var minX = (int)testZone.Points.Min(p => p.X);
			var maxX = (int)testZone.Points.Max(p => p.X);
			var minY = (int)testZone.Points.Min(p => p.Y);
			var maxY = (int)testZone.Points.Max(p => p.Y);

			int objNumber = 15;
			for (int i = 0; i < objNumber; i++)
			{
				var testCircle = new CircleOverlayEntryJsModel
				{
					Center = new UniPoint2f
					{
						X = rnd.Next(minX, maxX),
						Y = rnd.Next(minY, maxX)
					},
					Size = 4
				};
				testCircles.Add(testCircle);

				var speedCoeff = 0.5f + rnd.NextFloat() * 0.5f;
				speedCoeffs.Add(speedCoeff);
			}

			foreach (var circle in testCircles)
			{
				if (UniPolygon2f.IsInside(testZone.Points, circle.Center))
				{
					var circleSerialized = JsonSerializer.Serialize(circle, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

					var updateDetails = new MapOverlayUpdateDetails()
					{
						Type = MapOverlayUpdateDetails.AddOrUpdateUpdateType,
						OverlayEntry = circleSerialized,
					};

					await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, updateDetails, false });
				}
			}

			//_ = Task.Run(async () =>
			//  {
			while (true)
			{
				await Task.Delay(3000);

				foreach (var testCircle in testCircles)
				{
					float rndShiftX = rnd.Next(-25, 25);
					float rndShiftY = rnd.Next(-25, 25);
					rndShiftX *= speedCoeffs[testCircles.IndexOf(testCircle)];
					rndShiftY *= speedCoeffs[testCircles.IndexOf(testCircle)];

					var center = new UniPoint2f(testCircle.Center.X, testCircle.Center.Y);
					if (testCircle.Center.X + rndShiftX > maxX)
					{
						center.X = maxX;
					}
					else if (testCircle.Center.X + rndShiftX < minX)
					{
						center.X = minX;
					}
					else
					{
						center.X = testCircle.Center.X + rndShiftX;
					}

					if (testCircle.Center.Y + rndShiftY > maxY)
					{
						center.Y = maxY;
					}
					else if (testCircle.Center.Y + rndShiftY < minY)
					{
						center.Y = minY;
					}
					else
					{
						center.Y = testCircle.Center.Y + rndShiftY;
					}

					if (UniPolygon2f.IsInside(testZone.Points, center))
					{
						testCircle.Center = center;
						var circleSerialized = JsonSerializer.Serialize(testCircle, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

						var updateDetails = new MapOverlayUpdateDetails()
						{
							Type = MapOverlayUpdateDetails.AddOrUpdateUpdateType,
							OverlayEntry = circleSerialized,
						};

						await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, updateDetails });
					}
				}

			}
			//});
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

			await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, updateDetails });

			this.ShowingControlPropertyGrid.Value = false;
		}

		private object GetJsModelForOverlayEntry(OverlayEntry entry)
		{
			var entryType = entry.GetType();

			object model = null;

			if(entryType == typeof(ZoneOverlayEntry))
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
