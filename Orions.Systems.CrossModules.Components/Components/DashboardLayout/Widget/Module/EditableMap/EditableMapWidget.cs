using Orions.Infrastructure.HyperMedia.MapOverlay;
using Orions.Node.Common;
using System;

namespace Orions.Systems.CrossModules.Components.Components.DashboardLayout.Widget.Module.EditableMap
{
	public class EditableMapWidget : DashboardWidget, IDashboardWidget
	{
		
		[HyperDocumentId.DocumentType(typeof(MapOverlay))]
		public HyperDocumentId? MapOverlayId { get; set; }

		[UniBrowsable(UniBrowsableAttribute.EditTypes.MultiLineText)]
		public string SvgHtmlString { get; set; } = @"<svg viewBox=""0 0 300 100"" xmlns=""http://www.w3.org/2000/svg"" fill=""grey""></svg>";

		public bool IsReadOnly { get; set; } = false;

		public string DefaultZoneColor { get; set; } = "#FF0000";
		public string DefaultCameraColor { get; set; } = "#00FF00";
		public string DefaultCircleColor { get; set; } = "#0000FF";

		public EditableMapWidget()
		{
			this.Label = "Editable Map Widget";
		}
	}
}
