using Orions.Common;
using Orions.Infrastructure.HyperMedia.MapOverlay;
using System;
using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor.JsModel
{
	public class CameraOverlayEntryJsModel : BaseOverlayEntryJsModel
	{
		public List<UniPoint2f> Points { get; set; } = new List<UniPoint2f>();

		public SvgTransformMatrix TransformMatrix { get; set; }

		public override string EntryType => "camera";

		public static CameraOverlayEntryJsModel CreateFromDomainModel(CameraOverlayEntry cameraDomain)
		{
			var model = new CameraOverlayEntryJsModel
			{
				Points = cameraDomain.Points,
				TransformMatrix = cameraDomain.TransformMatrix
			};

			BaseOverlayEntryJsModel.MapBasePropertiesFromDomainModel(cameraDomain, model);

			return model;
		}

		public CameraOverlayEntry ToDomainModel()
		{
			var domain = new CameraOverlayEntry();

			BaseOverlayEntryJsModel.MapBasePropertiesToDomainModel(domain, this);

			domain.Points = this.Points;
			domain.TransformMatrix = this.TransformMatrix;
			
			return domain;
		}
	}
}