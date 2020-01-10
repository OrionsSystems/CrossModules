using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Providers
{
	public class CustomAuthenticationStateProvider : AuthenticationStateProvider
	{
		private ILocalStorageService _localStorageService;

		public CustomAuthenticationStateProvider(ILocalStorageService localStorageService)
		{
			_localStorageService = localStorageService;
		}

		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var userName = await _localStorageService.GetItemAsync<string>("userName");

			ClaimsIdentity identity;

			if (!string.IsNullOrWhiteSpace(userName))
			{
				identity = new ClaimsIdentity(new[]
				{
					new Claim(ClaimTypes.Name, userName),
				}, "form_auth_type");
			}
			else
			{
				identity = new ClaimsIdentity();
			}

			var user = new ClaimsPrincipal(identity);

			return await Task.FromResult(new AuthenticationState(user));
		}

		public async Task Authenticated(string userName, IHyperAuthenticationInfo authInfo)
		{
			if (authInfo == null)
				throw new ArgumentException(nameof(authInfo));

			await _localStorageService.SetItemAsync("userName", userName);
			await _localStorageService.SetItemAsync("authToken", authInfo.Auth.Token);

			var identity = new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.Name, userName),
			}, "form_auth_type");

			var user = new ClaimsPrincipal(identity);

			NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
		}

		public async Task LogOut()
		{
			await _localStorageService.RemoveItemAsync("userName");
			await _localStorageService.RemoveItemAsync("authToken");

			var identity = new ClaimsIdentity();

			var user = new ClaimsPrincipal(identity);

			NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
		}
	}
}
