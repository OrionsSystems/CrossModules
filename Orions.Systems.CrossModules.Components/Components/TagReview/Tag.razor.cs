using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
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
      public IHyperArgsSink HyperStore { get; set; }

      [Parameter]
      public int DashApiPort { get; set; }

      [Inject]
      public IJSRuntime JsRuntime { get; set; }

      [Parameter]
      public bool ShowFragmentAndSlice
      {
         get
         {
            return this.Vm.ShowFragmentAndSlice;
         }

         set
         {
            if (Vm != null)
               Vm.ShowFragmentAndSlice = value;
         }
      }

      [Parameter]
      public string FabricService
      {
         get => this.Vm.FabricService;

         set
         {
            if (Vm != null)
               Vm.FabricService = value;
         }
      }


      public TagBase()
      {
      }

      protected override async Task OnParametersSetAsync()
      {
         if (Tag != null && HyperStore != null)
         {
            await this.Vm.Initialize(Tag, HyperStore, DashApiPort);
         }

         await base.OnParametersSetAsync();
      }

   }
}
