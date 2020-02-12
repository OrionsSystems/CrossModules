using Orions.Common;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Components.Components.SVGMapEditor;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orions.Infrastructure.HyperMedia.MapOverlay;

namespace Orions.Systems.CrossModules.Components.Components.DashboardLayout.Widget.Module.EditableMap
{
	[Config(typeof(EditableMapWidget))]
	public class EditableMapWidgetVm : WidgetVm<EditableMapWidget>
	{
		public SVGMapEditorVm EditorVm { get; set; }

		public string SvgHtmlString { get; set; }

		public async Task Initialize()
		{
			this.SvgHtmlString = this.Widget.SvgHtmlString;

			var vm = new SVGMapEditorVm();

			vm.HyperArgsSink = this.HyperStore;
			vm.MapOverlayId = this.Widget.MapOverlayId;
			vm.IsReadOnly = this.Widget.IsReadOnly;
			vm.DefaultZoneColor = this.Widget.DefaultZoneColor;
			vm.DefaultCameraColor = this.Widget.DefaultCameraColor;
			vm.DefaultCircleColor = this.Widget.DefaultCircleColor;

			EditorVm = vm;

			vm.OnMapOverlayIdSet = async (HyperDocumentId? id) =>
			{
				this.Widget.MapOverlayId = id;
				await this.DashboardVm.SaveChangesAsync();
			};
		}
	}
}
