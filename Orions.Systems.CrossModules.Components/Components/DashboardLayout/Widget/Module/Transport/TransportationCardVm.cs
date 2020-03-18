using Orions.Common;
using Orions.Infrastructure.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(TransportationCardWidget))]
	public class TransportationCardVm : ReportWidgetVm<TransportationCardWidget>
	{
		public List<CardItem> Data { get; set; } = new List<CardItem>();

		public TransportationCardVm()
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

		public void OnChangeDataSource()
		{
			if (ReportChartData == null) return;

			var total = 0;
			Data.Clear();
			foreach (var series in ReportChartData.Series)
			{
				var item = new CardItem()
				{
					Title = series.Name,
					Value = series.Data.LastOrDefault()?.Value,
					SvgIcon = series.SvgIcon()
				};

				Data.Add(item);

				total += item.IntValue.GetValueOrDefault();
			}

			if (total != 0)
			{
				foreach (var item in Data)
				{
					if (item.DoubleValue == null || item.DoubleValue.GetValueOrDefault() == 0) continue;

					item.Percentage = (item.DoubleValue.GetValueOrDefault() / total) * 100;
				}
			}
		}
   }
}
