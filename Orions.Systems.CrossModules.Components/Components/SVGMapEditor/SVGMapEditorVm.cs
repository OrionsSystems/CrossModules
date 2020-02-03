using Orions.Common;
using System.Collections.Generic;
using Orions.Infrastructure.HyperMedia.MapOverlay;
using System.Threading.Tasks;
using Orions.Node.Common;
using System;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor
{
	public class SVGMapEditorVm : BlazorVm
	{
		public IHyperArgsSink HyperArgsSink { get; set; }
		public string MapOverlayId { get; set; }

		public ViewModelProperty<MapOverlay> MapOverlay { get; set; } = new ViewModelProperty<MapOverlay>(new Infrastructure.HyperMedia.MapOverlay.MapOverlay());

		public Func<string, Task> OnMapOverlayIdSet { get; set; }

		public async Task Initialize()
		{
			if(MapOverlayId != null)
			{
				var docId = HyperDocumentId.Create<MapOverlay>(MapOverlayId);

				var retrieveArgs = new RetrieveHyperDocumentArgs(docId);

				var hyperDocument = await HyperArgsSink.ExecuteAsync(retrieveArgs);

				var mapOverlay = hyperDocument.GetPayload<MapOverlay>();

				this.MapOverlay.Value = mapOverlay;
			}
			else
			{
				var doc = new HyperDocument(MapOverlay.Value);

				var storeDocArgs = new StoreHyperDocumentArgs(doc);

				await this.HyperArgsSink.ExecuteAsync(storeDocArgs);

				if(this.OnMapOverlayIdSet != null)
				{
					await OnMapOverlayIdSet.Invoke(doc.Id.Id);
				}
			}
		}

		public async Task SaveMapOverlay(MapOverlay overlay)
		{
			MapOverlay.Value = overlay;

			var doc = new HyperDocument(MapOverlay.Value);

			var storeDocArgs = new StoreHyperDocumentArgs(doc);

			await this.HyperArgsSink.ExecuteAsync(storeDocArgs);
		}
	}
}
