using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperMedia.MapOverlay;
using Orions.Node.Common;
using System;

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor.JsModel
{
	public class ZoneOverlayEntryJsModel : BaseOverlayEntryJsModel
	{
		public UniPoint2f[] Points { get; set; } = new UniPoint2f[4];

		public string Alias { get; set; }

		public string MetadataSetId { get; set; }

		public string FixedCameraEnhancementId { get; set; }

		public override string EntryType => "zone";

		public static ZoneOverlayEntryJsModel CreateFromDomainModel(ZoneOverlayEntry zoneDomain)
		{
			var model = new ZoneOverlayEntryJsModel
			{
				Alias = zoneDomain.Alias,
				FixedCameraEnhancementId = zoneDomain.FixedCameraEnhancementId.HasValue ? zoneDomain.FixedCameraEnhancementId.Value.Id : null,
				Points = zoneDomain.Points,
				MetadataSetId = zoneDomain.MetadataSetId.HasValue ? zoneDomain.MetadataSetId.Value.Id : null
			};

			BaseOverlayEntryJsModel.MapBasePropertiesFromDomainModel(zoneDomain, model);

			return model;
		}

		public ZoneOverlayEntry ToDomainModel()
		{
			var domain = new ZoneOverlayEntry();

			BaseOverlayEntryJsModel.MapBasePropertiesToDomainModel(domain, this);

			domain.Points = this.Points;
			domain.Alias = this.Alias;
			domain.MetadataSetId = this.MetadataSetId != null ? HyperDocumentId.Create<HyperMetadataSet>(this.MetadataSetId) : (HyperDocumentId?)null;
			domain.FixedCameraEnhancementId = this.FixedCameraEnhancementId != null ? HyperDocumentId.Create<FixedCameraEnhancedData>(this.FixedCameraEnhancementId) : (HyperDocumentId?)null;

			return domain;
		}
	}
}