using Orions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orions.Systems.CrossModules.Components
{
    [Config(typeof(CarouselWidget))]
    public class CarouselVm : WidgetVm<CarouselWidget>
    {
        public CarouselVm()
        {
        }
    }
}
