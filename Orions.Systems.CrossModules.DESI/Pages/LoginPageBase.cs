using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Orions.Systems.CrossModules.Desi.Debug.Infrastructure;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.Desi.Core.ViewModels;

namespace Orions.Systems.CrossModules.Desi.Debug.Pages
{
	public class LoginPageBase : ComponentBase
	{
		[Inject]
		public BlazorDependencyResolver DependencyResolver { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		protected ISettingsStorage SettingsStorage { get; set; }

		public AuthenticationViewModel Vm { get; set; }

		public LoginPageBase()
		{
		}

		public Task Initialize()
		{
			SettingsStorage = DependencyResolver.GetSettingsStorage();
			var authenticationSystem = DependencyResolver.GetAuthenticationSystem();

			Vm = new AuthenticationViewModel(authenticationSystem, new NavigationService(NavigationManager))
			{
				IsDevModeEnabled = false,
				IsStaySigned = true
			};

			return Task.CompletedTask;
		}
	}
}
