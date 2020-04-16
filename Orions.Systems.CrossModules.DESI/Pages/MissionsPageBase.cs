using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Core.ViewModels;
namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class MissionsPageBase : BaseViewModelComponent<MissionsViewModel>
	{
		protected override async Task OnInitializedAsyncSafe()
		{
			await Initialize();
		}

		public async Task Initialize()
		{
			var authSystem = DependencyResolver.GetAuthenticationSystem();
			var navigationService = DependencyResolver.GetNavigationService();

			if (authSystem.Store.Data.AuthenticationStatus == AuthenticationStatus.LoggedOut)
			{
				await navigationService.GoToLoginPage();
				return;
			}

			var missionSystem = DependencyResolver.GetMissionsExploitationSystem();
			this.Vm = new MissionsViewModel(
					navigationService,
					DependencyResolver.GetApiHelper(),
					DependencyResolver.GetDialogService(),
					DependencyResolver.GetAuthenticationSystem(),
					missionSystem
				);

			this.Vm.FetchMissionsCommand?.Execute(null);
		}
	}
}
