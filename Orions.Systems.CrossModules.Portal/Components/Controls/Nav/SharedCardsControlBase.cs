using Microsoft.AspNetCore.Components;

using Orions.Common;
using Orions.SDK;
using Orions.Systems.CrossModules.Portal.Domain;

using System.Collections.Generic;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.Components;

namespace Orions.Systems.CrossModules.Portal.Components
{
	public class SharedCardsControlBase : BaseBlazorComponent
	{
		[Parameter]
		public IEnumerable<NavigationEntryVm> CreateItemsSource { get; set; }

		[Parameter]
		public IEnumerable<NavigationEntryVm> ItemsSource { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
		}
	}
}
