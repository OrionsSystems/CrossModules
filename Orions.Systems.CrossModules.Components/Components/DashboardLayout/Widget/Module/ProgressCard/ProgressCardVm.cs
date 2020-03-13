using Orions.Common;
using Orions.Infrastructure.Reporting;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(ProgressCardWidget))]
	public class ProgressCardVm : ReportWidgetVm<ProgressCardWidget>
	{
		public ProgressCardVm()
		{
		}

		public async Task HandleOnClick(CardItem card)
		{
			if (card == null || string.IsNullOrWhiteSpace(card.Title)) return;

			if (!this.Widget.AllowFiltrationSource_TextCategory) return;

			var data = this.DashboardVm.ObtainFilterGroup(this.Widget);

			data.FilterLabels = new string[] { card.Title };
			data.FilterTarget = ReportFilterInstruction.Targets.Column;

			await this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();

		}
	}
}
