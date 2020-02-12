using Orions.Common;
using Orions.Infrastructure.HyperMedia.MapOverlay;
using System;

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor.JsModel
{
	public class CircleOverlayEntryJsModel : BaseOverlayEntryJsModel
	{
		public UniPoint2f Center { get; set; }
		public double Size { get; set; }
		public override string EntryType { get; } = "circle";

		internal static CircleOverlayEntryJsModel CreateFromDomainModel(CircleOverlayEntry circleDomain)
		{
			var model = new CircleOverlayEntryJsModel
			{
				Center = circleDomain.Center,
				Size = circleDomain.Size
			};

			BaseOverlayEntryJsModel.MapBasePropertiesFromDomainModel(circleDomain, model);

			return model;
		}

		public CircleOverlayEntry ToDomainModel()
		{
			var domain = new CircleOverlayEntry();

			BaseOverlayEntryJsModel.MapBasePropertiesToDomainModel(domain, this);

			domain.Center = this.Center;
			domain.Size = this.Size;

			return domain;
		}
	}
}