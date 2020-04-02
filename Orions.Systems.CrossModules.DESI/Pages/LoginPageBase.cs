using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.Desi.Core.ViewModels;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class LoginPageBase : DesiBaseComponent<AuthenticationViewModel>
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

		protected override async Task OnInitializedAsync()
		{
			SettingsStorage = DependencyResolver.GetSettingsStorage();
			var authenticationSystem = DependencyResolver.GetAuthenticationSystem();
			var navigationService = DependencyResolver.GetNavigationService();

			Vm = new AuthenticationViewModel(authenticationSystem, navigationService)
			{
			};

			await base.OnInitializedAsync();
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await InitializeJs();
			}

			base.OnAfterRender(firstRender);
		}

		private async Task InitializeJs()
		{
			var componentRef = DotNetObjectReference.Create(this);
			await JSRuntime.InvokeVoidAsync("Orions.LoginPage.initialize", new object[] { componentRef });
		}
	}
}
