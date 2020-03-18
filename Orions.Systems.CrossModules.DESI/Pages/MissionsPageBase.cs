using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Debug.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Core.ViewModels;
namespace Orions.Systems.CrossModules.Desi.Debug.Pages
{
	public class MissionsPageBase : DesiBaseComponent<MissionsViewModel>
	{
		protected override async Task OnInitializedAsync()
		{
			await Initialize();
		}

		public async Task Initialize()
		{
			var authSystem = DependencyResolver.GetAuthenticationSystem();

			if (authSystem.Store.Data.AuthenticationStatus == AuthenticationStatus.LoggedOut)
			{
				await new NavigationService(NavigationManager).GoToLoginPage();
				return;
			}

			var missionSystem = DependencyResolver.GetMissionsExploitationSystem();
			this.Vm = new MissionsViewModel(
					null,
					DependencyResolver.GetApiHelper(),
					DependencyResolver.GetDialogService(),
					DependencyResolver.GetAuthenticationSystem(),
					missionSystem
				);

			this.Vm.FetchMissionsCommand?.Execute(null);
		}
	}
}
