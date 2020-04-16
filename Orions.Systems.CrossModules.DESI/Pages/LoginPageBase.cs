using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.Desi.Core.ViewModels;
using Orions.Systems.Desi.Common.Authentication;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class LoginPageBase : BaseViewModelComponent<AuthenticationViewModel>
	{
		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		protected ISettingsStorage SettingsStorage { get; set; }

		public LoginPageBase()
		{
		}

		[JSInvokable]
		public void Login()
		{
			this.SettingsStorage.Save();
			Vm.LoginCommand.Execute(null);
		}

		protected override async Task OnInitializedAsyncSafe()
		{
			SettingsStorage = DependencyResolver.GetSettingsStorage();
			var authenticationSystem = DependencyResolver.GetAuthenticationSystem();
			var navigationService = DependencyResolver.GetNavigationService();

			Vm = new AuthenticationViewModel(authenticationSystem, navigationService)
			{
			};

			if(Vm.AuthenticationData.AuthenticationStatus == AuthenticationStatus.LoggedIn)
			{
				authenticationSystem.Controller.Dispatch(LogoutAction.Create());
			}
		}

		protected override async Task OnAfterRenderAsyncSafe(bool firstRender)
		{
			if (firstRender)
			{
				await InitializeJs();
			}
		}

		private async Task InitializeJs()
		{
			var componentRef = DotNetObjectReference.Create(this);
			await JSRuntime.InvokeVoidAsync("Orions.LoginPage.initialize", new object[] { componentRef });
		}
	}
}
