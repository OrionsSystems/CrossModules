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
      public ITagReviewContext Context { get; set; }

      public TagGridBase()
      {
      }

      protected override Task OnParametersSetAsync()
      {
         return base.OnParametersSetAsync();
      }
   }
}
