using Microsoft.AspNetCore.Components;
using Orions.Infrastructure.HyperMedia;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
    public class TagGridBase : BaseOrionsComponent
    {
        [Parameter]
        public List<HyperTag> HyperTags { get; set; }

		[Parameter]
		public int ColumnsNumber { get; set; } = 4;

		[Parameter]
        public NetStore HyperStore { get; set; }

		[Parameter]
		public int DashApiPort { get; set; }


		protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }
    }
}
