using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(DropNavWidget))]
	public class DropNavVm : WidgetVm<DropNavWidget>
	{
		public DropNavVm()
		{
		}

		public void OnSelectStory(string view)
		{
			var filter = this.DashboardVm.ObtainFilterGroup(this.Widget.FilterGroup);
			if (filter == null)
				return;

			if (string.IsNullOrEmpty(view) == false && filter.View != view)
			{
				filter.View = view;
				var t = this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();
			}
		}
	}
}
