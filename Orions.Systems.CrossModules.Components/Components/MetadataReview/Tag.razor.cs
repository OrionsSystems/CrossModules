using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Infrastructure.HyperMedia;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
    public class TagBase : BaseBlazorComponent<HyperTagVm>
    {
        [Parameter]
        public HyperTag Tag { get; set; }

        [Parameter]
        public NetStore HyperStore { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if(Tag != null && HyperStore != null)
            {
                await this.DataContext.Initialize(Tag, HyperStore);
            }

            await base.OnParametersSetAsync();
        }
        
    }
}
