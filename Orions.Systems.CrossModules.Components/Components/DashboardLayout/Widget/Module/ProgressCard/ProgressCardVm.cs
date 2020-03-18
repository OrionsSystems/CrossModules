using Orions.Common;
using Orions.Infrastructure.Reporting;
using System.Linq;
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

			var filters = new string[] { card.Title.ToUpper() };
			if (data.FilterLabels == null)
			{
				data.FilterLabels = filters;
			}
			else
			{
				data.FilterLabels = data.FilterLabels.Concat(filters).Distinct().ToArray();
			}

			data.FilterTarget = ReportFilterInstruction.Targets.Column;

			if (data.AllowDynamicFiltration)
			{
				await this.DashboardVm.UpdateDynamicWidgetsFilteringAsync();
			}

		}
	}
}
