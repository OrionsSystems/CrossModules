using Microsoft.AspNetCore.Components;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
    public class MetadataReviewBase : BaseBlazorComponent<MetadataReviewVm>
    {
        protected NetStore _store;

        [Parameter]
        public string MetadataSetId { get; set; }

		[Parameter]
		public UniFilterData Filter { get; set; }

		[Parameter]
		public int ColumnsNumber { get; set; }


		protected override async Task OnParametersSetAsync()
		{
			if(Filter != null)
			{
				await this.Vm.FilterTags(Filter);
			}

			await this.DataContext.Initialize(_store, MetadataSetId, ColumnsNumber * 2);

			await base.OnParametersSetAsync();
		}

		protected override async Task OnInitializedAsync()
        {
            _store = await NetStore.ConnectAsyncThrows("http://vladimir:654321@usbellods01wan.orionscloud.com:4580/Execute");

            await base.OnInitializedAsync();
        }
    }
}
