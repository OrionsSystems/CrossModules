using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Portal.Providers
{
	public class CustomAuthenticationStateProvider : AuthenticationStateProvider
	{
		private ILocalStorageService _localStorageService;

		public const string authUsernameLabel = "authUsername";
		public const string authTokenLabel = "authToken";
		public const string authUriLabel = "authUri";

		public CustomAuthenticationStateProvider(ILocalStorageService localStorageService)
		{
			_localStorageService = localStorageService;
		}

		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var authUsername = await _localStorageService.GetItemAsync<string>(authUsernameLabel);
			var authToken = await _localStorageService.GetItemAsync<string>(authTokenLabel);
			var authUri = await _localStorageService.GetItemAsync<string>(authUriLabel);

			ClaimsIdentity identity;

			if (!string.IsNullOrWhiteSpace(authToken)
				&& !string.IsNullOrWhiteSpace(authUsername)
				&& !string.IsNullOrWhiteSpace(authUri))
			{
				identity = new ClaimsIdentity(new[]
				{
					new Claim(ClaimTypes.Name, authUsername),
					new Claim(ClaimTypes.Sid, authToken),
					new Claim(ClaimTypes.Uri, authUri),
				}, "form_auth_type");
			}
			else
			{
				identity = new ClaimsIdentity();
			}

			var user = new ClaimsPrincipal(identity);

			return await Task.FromResult(new AuthenticationState(user));
		}

		public async Task Authenticated(
			string username,
			string uri,
			IHyperAuthenticationInfo authInfo)
		{
			if (authInfo == null)
				throw new ArgumentException(nameof(authInfo));

			if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException(nameof(username));

			if (string.IsNullOrWhiteSpace(uri))
				throw new ArgumentException(nameof(uri));

			await _localStorageService.RemoveItemAsync(authUsernameLabel);
			await _localStorageService.RemoveItemAsync(authTokenLabel);
			await _localStorageService.RemoveItemAsync(authUriLabel);

			await _localStorageService.SetItemAsync(authUsernameLabel, username);			
			await _localStorageService.SetItemAsync(authTokenLabel, authInfo.Auth.Token);		
			await _localStorageService.SetItemAsync(authUriLabel, uri);

			var identity = new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.Name, username),
				new Claim(ClaimTypes.Sid, authInfo.Auth.Token),
				new Claim(ClaimTypes.Uri, uri),
			}, "form_auth_type");

			var user = new ClaimsPrincipal(identity);

			NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
		}

		public async Task LogOut()
		{
			await _localStorageService.RemoveItemAsync(authUsernameLabel);
			await _localStorageService.RemoveItemAsync(authTokenLabel);
			await _localStorageService.RemoveItemAsync(authUriLabel);

			var identity = new ClaimsIdentity();

			var user = new ClaimsPrincipal(identity);

			NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
		}
	}
}