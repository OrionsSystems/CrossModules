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

        protected override async Task OnInitializedAsync()
        {
            _store = await NetStore.ConnectAsyncThrows("http://vladimir:654321@usnbods01wan.orionscloud.com:8600/Execute");

            await this.DataContext.Initialize(_store);

            DataContext.PageSize.PropertyChanged += PageSize_PropertyChanged;

            await base.OnInitializedAsync();
        }

        private void PageSize_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.InvokeAsync(async () =>
            {
                await DataContext.LoadTotalPages();
                await DataContext.LoadHyperTags();

                this.StateHasChanged();
            });
        }
    }
}
