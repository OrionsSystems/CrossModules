using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Portal.Providers
{
	public class CustomAuthenticationStateProvider : AuthenticationStateProvider
	{
		private ProtectedSessionStorage _protectedSessionStorage;

		public const string AuthUsernameLabel = "_u_v1";
		public const string AuthTokenLabel = "_t_v1";
		public const string AuthUriLabel = "_r_v1";
		public const string FormAuthTypeLabel = "form_auth_type";

		public CustomAuthenticationStateProvider(ProtectedSessionStorage protectedSessionStorage)
		{
			_protectedSessionStorage = protectedSessionStorage;
		}

		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var authUsername = await _protectedSessionStorage.GetAsync<string>(AuthUsernameLabel);
			var authToken = await _protectedSessionStorage.GetAsync<string>(AuthTokenLabel);
			var authUri = await _protectedSessionStorage.GetAsync<string>(AuthUriLabel);

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

			await _protectedSessionStorage.DeleteAsync(AuthUsernameLabel);
			await _protectedSessionStorage.DeleteAsync(AuthTokenLabel);
			await _protectedSessionStorage.DeleteAsync(AuthUriLabel);

			await _protectedSessionStorage.SetAsync(AuthUsernameLabel, username);
			await _protectedSessionStorage.SetAsync(AuthTokenLabel, authInfo.Auth.Token);
			await _protectedSessionStorage.SetAsync(AuthUriLabel, uri);

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
			await _protectedSessionStorage.DeleteAsync(AuthUsernameLabel);
			await _protectedSessionStorage.DeleteAsync(AuthTokenLabel);
			await _protectedSessionStorage.DeleteAsync(AuthUriLabel);

			var identity = new ClaimsIdentity();

			var user = new ClaimsPrincipal(identity);

			NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
		}
	}
}