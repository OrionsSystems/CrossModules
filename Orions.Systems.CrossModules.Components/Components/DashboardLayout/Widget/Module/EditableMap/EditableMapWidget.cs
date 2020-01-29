using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components.Components.DashboardLayout.Widget.Module.EditableMap
{
	public class EditableMapWidget : DashboardWidget, IDashboardWidget
	{
		public List<SVGMapEditor.CameraControl> Cameras { get; set; }

		public EditableMapWidget()
		{
			this.Label = "Editable Map Widget";
		}
	}
}
