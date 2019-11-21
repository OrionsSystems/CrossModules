using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
	public class VanilaColorPickerConfig
	{
		public string ParentId { get; set; }

		public string Popup { get; set; } = "right";

		public string Layout { get; set; } = "default";

		public bool Alpha { get; set; } = true;

		public bool Editor { get; set; } = true;

		public string EditorFormat { get; set; } = "hex";

		public bool CancelButton { get; set; } = false;

		public string Color { get; set; }
	}
}
