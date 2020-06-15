using Microsoft.AspNetCore.Components;

using Orions.Common;
using Orions.SDK;
using Orions.Systems.CrossModules.Components;

using System.Threading.Tasks;

using Orions.Systems.CrossModules.Portal.Services;

namespace Orions.Systems.CrossModules.Portal.Components
{
	public class SolutionExplorerControlBase : BaseOrionsComponent, ICardControl
	{
		[CascadingParameter]
		protected SolutionVmEx Solution { get; set; }

		public AdvancedObservableCollection<ModuleVm> ItemsSource { get { return Solution.ActiveModules; } }

		protected IModuleVm SelectedItem { get { return Solution?.SelectedModuleVmProp?.Value; } }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
		}

	}
}
