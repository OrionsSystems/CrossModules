using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Orions.Systems.CrossModules.Portal.Models
{
	public class RedirectToLogin : ComponentBase
	{
		[Inject]
		protected NavigationManager NavigationManager { get; set; }

		[CascadingParameter]
		private Task<AuthenticationState> AuthenticationStateTask { get; set; }

		protected override async Task OnInitializedAsync()
		{
			var authenticationState = await AuthenticationStateTask;

			if (authenticationState?.User?.Identity is null || !authenticationState.User.Identity.IsAuthenticated)
			{
				var returnUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

				if (string.IsNullOrWhiteSpace(returnUrl))
					NavigationManager.NavigateTo("login", true);
				else
					NavigationManager.NavigateTo($"login?returnUrl={returnUrl}", true);
			}
		}
	}
}
