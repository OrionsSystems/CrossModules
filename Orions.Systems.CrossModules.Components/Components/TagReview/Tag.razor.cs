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
      public ITagReviewContext Context
      {
         get
         {
            return this.Vm.Context;
         }

         set
         {
            if (Vm != null)
               Vm.Context = value;
         }
      }

      [Inject]
      public IJSRuntime JsRuntime { get; set; }

      public TagBase()
      {
      }

      protected override async Task OnParametersSetAsync()
      {
         if (Tag != null)
            await this.Vm.Initialize(Tag);

         await base.OnParametersSetAsync();
      }

   }
}
