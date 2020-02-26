using System;

using Orions.Infrastructure.HyperMedia.MapOverlay;
using Orions.Node.Common;

using static Orions.Systems.CrossModules.Components.Components.SVGMapEditor.SVGMapEditorVm;

namespace Orions.Systems.CrossModules.Components.Components.DashboardLayout.Widget.Module.EditableMap
{
	public class EditableMapWidget : DashboardWidget
	{

		[HyperDocumentId.DocumentType(typeof(MapOverlay))]
		public HyperDocumentId? MapOverlayId { get; set; }

		[UniBrowsable(UniBrowsableAttribute.EditTypes.MultiLineText)]
		public string SvgHtmlString { get; set; } = @"<svg viewBox=""0 0 300 100"" xmlns=""http://www.w3.org/2000/svg"" fill=""grey""></svg>";

		public bool IsReadOnly { get; set; } = false;
		public int TagRequestMaxCountLimit { get; set; }

		public TagDateRangeFilterOptions TagDateRangeFilter { get; set; } = new TagDateRangeFilterOptions();

		public string DefaultZoneColor { get; set; } = "#FF0000";
		public string DefaultCameraColor { get; set; } = "#00FF00";
		public string DefaultCircleColor { get; set; } = "#0000FF";

		public MapPlaybackCache MapPlaybackCache { get; set; }
		public MapPlaybackOptions MapPlaybackOptions { get; set; }

		public HeatmapRenderingMode HeatmapMode { get; set; } = HeatmapRenderingMode.Masks;
		public bool HeatmapCustomNormalization { get; set; } = false;
		public uint HeatmapNormalizationMinOverlaps { get; set; } = 10;
		public uint HeatmapNormalizationMaxOverlaps { get; set; } = 100;

		public EditableMapWidget()
		{
			this.Label = "Editable Map Widget";
		}
	}
}
