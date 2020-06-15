using Microsoft.AspNetCore.Components;

using Orions.Common;
using Orions.SDK;
using Orions.Systems.CrossModules.Portal.Domain;
using Orions.Systems.CrossModules.Components;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Components
{
	public class CardHostControlBase : BaseOrionsComponent
	{
		[Parameter]
		public NavigationEntryVm Source { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
		}
	}
}
