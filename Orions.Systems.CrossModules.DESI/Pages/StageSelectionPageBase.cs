using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Core.ViewModels;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class StageSelectionPageBase : BaseViewModelComponent<StageSelectionViewModel>
	{
		protected override async Task OnInitializedAsyncSafe()
		{
			var authSystem = DependencyResolver.GetAuthenticationSystem();
			var navigationService = DependencyResolver.GetNavigationService();

			if (authSystem.Store.Data.AuthenticationStatus == AuthenticationStatus.LoggedOut)
			{
				await navigationService.GoToLoginPage();

				return;
			}

			this.Vm = new StageSelectionViewModel(
				DependencyResolver.GetMissionsExploitationSystem(),
				DependencyResolver.GetNavigationService()
			);
		}

		public void Switch_Toggled(PhaseModel phase)
		{
			phase.IsSelected = !phase.IsSelected;
			this.Vm.ValidateStageSelection();
		}
	}
}
