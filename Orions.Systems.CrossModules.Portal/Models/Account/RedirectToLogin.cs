using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Portal.Providers;

namespace Orions.Systems.CrossModules.Portal.Models
{
	public class RedirectToLogin : ComponentBase
	{
		[Inject]
		protected NavigationManager NavigationManager { get; set; }

		[CascadingParameter]
		private Task<AuthenticationState> AuthenticationStateTask { get; set; }

		[CascadingParameter(Name = "Provider")]
		private AuthenticationStateProvider Provider { get; set; }

		protected override async Task OnInitializedAsync()
		{
			var authenticationState = await AuthenticationStateTask;

			if (authenticationState?.User?.Identity is null || !authenticationState.User.Identity.IsAuthenticated)
			{
				var returnUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

				var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);

				QueryHelpers.ParseQuery(uri.Query).TryGetValue("token", out var token);
				QueryHelpers.ParseQuery(uri.Query).TryGetValue("connstr", out var connectionString);

				if (!string.IsNullOrWhiteSpace(token)
					&& !string.IsNullOrWhiteSpace(connectionString))
				{
					var authentication = new HyperAuthenticationInfo()
					{
						Auth = new Node.Common.HyperArgsAuthentication()
						{
							Token = token
						}
					};

					await ((CustomAuthenticationStateProvider)Provider).Authenticated(
						"external", connectionString, authentication);

					NavigationManager.NavigateTo($"{returnUrl}", true);

					return;
				}

				if (string.IsNullOrWhiteSpace(returnUrl))
					NavigationManager.NavigateTo("login", true);
				else
					NavigationManager.NavigateTo($"login?returnUrl={returnUrl}", true);
			}
		}
	}
}
