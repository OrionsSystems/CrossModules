using Orions.Common;

using System.Threading.Tasks;


namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(DropNavWidget))]
	public class DropNavVm : WidgetVm<DropNavWidget>
	{
		public DropNavVm()
		{
		}

		public async Task OnSelectStory(string view)
		{
			var filter = this.DashboardVm.ObtainFilterGroup(this.Widget.FilterGroup);
			if (filter == null)
				return;

			if (!string.IsNullOrEmpty(view) && filter.View == view) return;

			if (!string.IsNullOrEmpty(view) && filter.View != view)
				filter.View = view;

			if (string.IsNullOrWhiteSpace(view))
				filter.View = string.Empty;

			await DashboardVm.UpdateDynamicWidgetsFilteringAsync();
		}
	}
}
