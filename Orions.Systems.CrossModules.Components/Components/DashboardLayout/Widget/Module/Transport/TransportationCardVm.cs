using Orions.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(TransportationCardWidget))]
	public class TransportationCardVm : ReportWidgetVm<TransportationCardWidget>
	{
		public List<CardItem> Data { get; set; } = new List<CardItem>();

		public TransportationCardVm()
		{

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

		public int GetAveragePeoplePerCar()
      {
         //TODO 
         var rnd = new Random();
         return rnd.Next(0, 20);
      }

      public int GetAverageFamilyePerCar()
      {
         //TODO 
         var rnd = new Random();
         return rnd.Next(0, 20);
      }
   }
}
