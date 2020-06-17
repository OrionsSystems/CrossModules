using Microsoft.AspNetCore.Components;

using Orions.SDK;
using Orions.Systems.CrossModules.Components;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Components
{
	public partial class SharedCardsControl : BaseBlazorComponent
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
