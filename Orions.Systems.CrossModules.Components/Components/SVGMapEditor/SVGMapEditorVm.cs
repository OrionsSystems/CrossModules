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

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor
{
	public class SVGMapEditorVm : BlazorVm
	{
		public IHyperArgsSink HyperArgsSink { get; set; }
		public HyperDocumentId? MapOverlayId { get; set; }
		public bool IsReadOnly { get; set; }

		public ViewModelProperty<MapOverlay> MapOverlay { get; set; } = new ViewModelProperty<MapOverlay>(new Infrastructure.HyperMedia.MapOverlay.MapOverlay());

		public IJSRuntime JsRuntime { get; set; }

		public Func<HyperDocumentId?, Task> OnMapOverlayIdSet { get; set; }

		private string _componentContainerId;
		public async Task Initialize(string componentContainerId)
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
		}

		public async Task TestLiveUpdate()
		{
			var testZone = this.MapOverlay.Value.Entries.First(e => e as ZoneOverlayEntry != null) as ZoneOverlayEntry;

			var testCircles = new List<CircleOverlayEntry>();
			var speedCoeffs = new List<float>();
			Random rnd = new Random();

			var minX = (int)testZone.Points.Min(p => p.X);
			var maxX = (int)testZone.Points.Max(p => p.X);
			var minY = (int)testZone.Points.Min(p => p.Y);
			var maxY = (int)testZone.Points.Max(p => p.Y);

			int objNumber = 15;
			for (int i = 0; i < objNumber; i++)
			{
				var testCircle = new CircleOverlayEntry
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
						EntryType = "circle"
					};

					await JsRuntime.InvokeAsync<object>("window.Orions.SvgMapEditor.update", new object[] { this._componentContainerId, updateDetails, false });
				}
			}

			//_ = Task.Run(async () =>
			//  {
			while (true)
			{
				await Task.Delay(1000);

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
							EntryType = "circle"
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
	}

	public class MapOverlayUpdateDetails
	{
		public const string AddOrUpdateUpdateType = "addOrUpdate";
		public const string DeleteUpdateType = "delete";

		public string Type { get; set; }

		public string EntryType { get; set; }
		public string OverlayEntry { get; set; }
	}
}
