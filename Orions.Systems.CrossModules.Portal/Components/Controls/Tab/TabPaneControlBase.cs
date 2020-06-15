using Microsoft.AspNetCore.Components;

using Orions.Common;
using Orions.SDK;
using Orions.Systems.CrossModules.Components;

using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Components
{
	public class TabPaneControlBase : BaseOrionsComponent
	{
		[Parameter]
		public TabVm SelectedItem { get; set; }

		[Parameter]
		public IReadOnlyObservableCollection<TabVm> ItemsSource { get; set; }

		[Parameter]
		public TabEntity.DockModes DockMode { get; set; } = TabEntity.DockModes.Main;

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
		}
	}
}
