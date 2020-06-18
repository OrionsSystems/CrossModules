using Microsoft.AspNetCore.Components;

using System.Threading.Tasks;
using Microsoft.JSInterop;
using Orions.Common;
using Microsoft.AspNetCore.WebUtilities;
using Orions.Systems.CrossModules.Portal.Providers;
using Microsoft.AspNetCore.Components.Authorization;
using Orions.Systems.CrossModules.Portal.Services;
using Orions.Systems.CrossModules.Components;
using Orions.SDK;

namespace Orions.Systems.CrossModules.Portal.Components
{
	public class PortalLayoutComponent : LayoutComponentBase
	{
		[Inject]
		protected IJSRuntime JsRuntime { get; set; }

		[Inject]
		protected NavigationManager NavigationManager { get; set; }

		[Inject]
		protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }

		[Inject]
		protected CustomSettingsProvider CustomSettingsProvider { get; set; }

		[Inject]
		protected SolutionVmEx Solution { get; set; }

		protected string VersionLabel { get; set; }

		protected bool IsEmbedded { get; set; }

		protected string CurrentTheme { get; set; }

		protected Toast ToastRef { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			if (Solution == null) {
				Solution = new SolutionVmEx();
			}

			Solution.EntityProp.Value = new SolutionEntity();

			Solution.EntityProp.Value.EmbeddedNodeActive = false;

			Solution.OnStateChanged += ReloadState;

			CurrentTheme = await GetTheme();

			var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
			if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("embed", out var token))
			{
				IsEmbedded = true;
			}

			if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("theme", out var value))
			{
				CurrentTheme = value;
			}

			VersionLabel = ReflectionHelper.Instance.SDKVersion;
		}

		protected async Task Logout()
		{
			await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).LogOut();

			NavigationManager.NavigateTo("/login");
		}

		private async Task<string> GetTheme()
		{
			var theme = await CustomSettingsProvider.GetParameter(CustomSettingsProvider.THEME_KEY);

			if (string.IsNullOrWhiteSpace(theme)) return CustomSettingsProvider.THEME_WHITE;

			return theme;
		}

		private void ReloadState()
		{
			StateHasChanged();
		}
	}
}
