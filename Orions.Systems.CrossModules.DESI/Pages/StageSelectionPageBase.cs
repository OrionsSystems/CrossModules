using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Core.ViewModels;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class StageSelectionPageBase : DesiBaseComponent<StageSelectionViewModel>
	{
		protected override Task OnInitializedAsync()
		{
			var vm = new StageSelectionViewModel(
				DependencyResolver.GetMissionsExploitationSystem(),
				DependencyResolver.GetNavigationService()
			);

			return base.OnInitializedAsync();
		}
	}
}
