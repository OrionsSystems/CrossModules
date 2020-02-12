using Orions.Infrastructure.HyperMedia.MapOverlay;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor.JsModel
{
	public class MapOverlayJsModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public List<ZoneOverlayEntryJsModel> Zones { get; set; }
		public List<CircleOverlayEntryJsModel> Circles { get; set; }
		public List<CameraOverlayEntryJsModel> Cameras { get; set; }

		public static object CreateFromDomainModel(MapOverlay domainModel)
		{
			var model = new MapOverlayJsModel();
			model.Id = domainModel.Id;
			model.Name = domainModel.Name;
			model.Zones = domainModel.Entries.Where(e => e is ZoneOverlayEntry).Cast<ZoneOverlayEntry>().Select(z => ZoneOverlayEntryJsModel.CreateFromDomainModel(z)).ToList();
			model.Circles = domainModel.Entries.Where(e => e is CircleOverlayEntry).Cast<CircleOverlayEntry>().Select(z => CircleOverlayEntryJsModel.CreateFromDomainModel(z)).ToList();
			model.Cameras = domainModel.Entries.Where(e => e is CameraOverlayEntry).Cast<CameraOverlayEntry>().Select(z => CameraOverlayEntryJsModel.CreateFromDomainModel(z)).ToList();

			return model;
		}

		public MapOverlay ToDomainModel() 
		{
			var domain = new MapOverlay();

			domain.Id = this.Id;
			domain.Name = this.Name;
			domain.Entries = this.Zones.Select(z => z.ToDomainModel()).Cast<OverlayEntry>()
				.Union(this.Circles.Select(c => c.ToDomainModel()))
				.Union(this.Cameras.Select(c => c.ToDomainModel()))
				.ToList();

			return domain;
		} 
	}
}
