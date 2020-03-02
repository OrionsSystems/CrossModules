using Microsoft.AspNetCore.Components;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
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
      public IHyperArgsSink HyperStore { get; set; }

      [Parameter]
      public int DashApiPort { get; set; }

      [Parameter]
      public bool ShowFragmentAndSlice { get; set; } = true;

      [Parameter]
      public bool ExtractMode { get; set; } = true;

      [Parameter]
      public string FabricService { get; set; } = "";

      public TagGridBase()
      {
      }

      protected override Task OnParametersSetAsync()
      {
         return base.OnParametersSetAsync();
      }
   }
}
