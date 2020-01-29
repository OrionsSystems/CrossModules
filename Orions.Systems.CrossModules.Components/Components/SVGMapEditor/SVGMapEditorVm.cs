using Orions.Common;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor
{
	public class SVGMapEditorVm : BlazorVm
	{
		public ViewModelProperty<List<CameraControl>> Cameras { get; set; }
	}
}
