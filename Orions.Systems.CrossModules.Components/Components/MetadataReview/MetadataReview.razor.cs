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
         _store = await NetStore.ConnectAsyncThrows("http://vladimir:654321@usbellods01wan.orionscloud.com:4580/Execute");

         await this.Vm.Initialize(_store);

         // No need to do this, there is an embedded mechanism in BlazorComponent that scans all ViewModelProperties<> and updates the state.
         Vm.PageSize.PropertyChanged += PageSize_PropertyChanged;
         Vm.PageNumber.PropertyChanged += PageNumber_PropertyChanged;

         await base.OnInitializedAsync();
      }

      private void PageNumber_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         this.InvokeAsync(async () =>
         {
            await Vm.ChangePage(Vm.PageNumber); // This is already done in the razor
            this.StateHasChanged();
         });
      }

      private void PageSize_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         this.InvokeAsync(async () =>
         {
            await Vm.LoadTotalPages();
            await Vm.LoadHyperTags();

            this.StateHasChanged();
         });
      }
   }
}
