using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.JSInterop;

using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(CarouselWidget))]
	public class CarouselVm : WidgetVm<CarouselWidget>
	{
		public bool IsDataInitialized { get; set; } = false;
		public List<Carousels> Data { get; set; } = new List<Carousels>();

		public CarouselVm()
		{
		}

		public void InitData()
		{
			Data = this.Widget.Settings.Navigations.Select(x => new Carousels
			{
				Source = x.ImageAsBase64,
				Alt = x.View
			}).ToList();

			OnItemChange(Data[0].Alt);

			IsDataInitialized = true;
		}

		[JSInvokable]
		public Task Switched(int index)
		{
			OnItemChange(Data[index].Alt);

			return Task.CompletedTask;
		}

		public void OnItemChange(string view)
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
