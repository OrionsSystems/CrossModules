using System.Threading.Tasks;
using Microsoft.JSInterop;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.Desi.Core.ViewModels;
using Orions.Systems.Desi.Common.Authentication;
using Microsoft.AspNetCore.Components;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class LoginPageBase : BaseViewModelComponent<AuthenticationViewModel>
	{
		[Inject]
		public ISettingsStorage SettingsStorage { get; set; }

		[Inject]
		public IAuthenticationSystem AuthenticationSystem { get; set; }

		[JSInvokable]
		public void Login()
		{
			this.SettingsStorage.Save();
			Vm.LoginCommand.Execute(null);
		}

		protected override async Task OnInitializedAsyncSafe()
		{
			if(Vm.AuthenticationData.AuthenticationStatus == AuthenticationStatus.LoggedIn)
			{
				AuthenticationSystem.Controller.Dispatch(LogoutAction.Create());
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
