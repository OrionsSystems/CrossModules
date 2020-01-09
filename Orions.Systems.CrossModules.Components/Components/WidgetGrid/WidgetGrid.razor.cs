using Microsoft.AspNetCore.Components;
using Orions.Infrastructure.HyperMedia;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
    public class WidgetGridBase : BaseOrionsComponent
    {
        [Parameter]
        public List<HyperTag> HyperTags { get; set; }

        [Parameter]
        public NetStore HyperStore { get; set; }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }
    }
}
