using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Portal.Providers
{
	public class CustomAuthenticationStateProvider : AuthenticationStateProvider
	{
		private ILocalStorageService _storage;

		public const string AuthUsernameLabel = "_u_v1";
		public const string AuthTokenLabel = "_t_v1";
		public const string AuthUriLabel = "_r_v1";
		public const string FormAuthTypeLabel = "form_auth_type";

		public CustomAuthenticationStateProvider(ILocalStorageService storage)
		{
			_storage = storage;
		}

		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var authUsername = await _storage.GetItemAsync<string>(AuthUsernameLabel);
			var authToken = await _storage.GetItemAsync<string>(AuthTokenLabel);
			var authUri = await _storage.GetItemAsync<string>(AuthUriLabel);

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
				}, FormAuthTypeLabel);
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

			await _storage.RemoveItemAsync(AuthUsernameLabel);
			await _storage.RemoveItemAsync(AuthTokenLabel);
			await _storage.RemoveItemAsync(AuthUriLabel);

			await _storage.SetItemAsync(AuthUsernameLabel, username);
			await _storage.SetItemAsync(AuthTokenLabel, authInfo.Auth.Token);
			await _storage.SetItemAsync(AuthUriLabel, uri);

			var identity = new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.Name, username),
				new Claim(ClaimTypes.Sid, authInfo.Auth.Token),
				new Claim(ClaimTypes.Uri, uri),
			}, FormAuthTypeLabel);

			var user = new ClaimsPrincipal(identity);

			NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
		}

		public async Task LogOut()
		{
			await _storage.RemoveItemAsync(AuthUsernameLabel);
			await _storage.RemoveItemAsync(AuthTokenLabel);
			await _storage.RemoveItemAsync(AuthUriLabel);

			var identity = new ClaimsIdentity();

			var user = new ClaimsPrincipal(identity);

			NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
		}
	}
}