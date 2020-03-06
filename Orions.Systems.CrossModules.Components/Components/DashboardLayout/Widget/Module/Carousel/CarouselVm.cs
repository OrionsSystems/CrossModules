using Orions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orions.Systems.CrossModules.Components
{
    [Config(typeof(CarouselWidget))]
    public class CarouselVm : ReportWidgetVm<CarouselWidget>
    {
        public List<Carousels> Data { get; set; } = new List<Carousels>();

        public CarouselVm()
        {
        }

        public void OnChangeDataSource()
        {
            if (ReportChartData == null) return;
            Data.Clear();

            foreach (var series in ReportChartData.Series)
            {

                for (int index = 0; index < series.Data.Count; index++)
                {
                    Carousels objCarousal;
                    if (Data.Count <= index)
                    {
                        objCarousal = new Carousels();
                        Data.Add(objCarousal);
                    }
                    else
                        objCarousal = Data[index];

                    PropertyInfo propertyInfo = objCarousal.GetType().GetProperty(series.Name);
                    propertyInfo.SetValue(objCarousal, Convert.ChangeType(series.Data[index].Value, propertyInfo.PropertyType), null);
                }
            }
        }
    }
}
