using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Components;
using Orions.Systems.CrossModules.Portal.Services;

using System;
using System.Linq;
using System.Threading.Tasks;


namespace Orions.Systems.CrossModules.Portal.Components
{
	[Authorize]
	public class PortalBaseComponent : BaseOrionsComponent
	{
		[CascadingParameter]
		protected Task<AuthenticationState> AuthenticationStateTask { get; set; }

		[CascadingParameter]
		protected Toast ToastRef { get; set; }

		[CascadingParameter]
		protected SolutionVmEx Solution { get; set; }

		[Inject]
		protected NavigationManager NavigationManager { get; set; }

		[Inject]
		protected IJSRuntime JSRuntime { get; set; }

		protected IHyperArgsSink HyperStore { get; set; }

		protected override async Task OnInitializedAsync()
		{
			var authState = await AuthenticationStateTask;

			var claimToken = authState.User.Claims.FirstOrDefault(it => it.Type == System.Security.Claims.ClaimTypes.Sid);

			var claimConnection = authState.User.Claims.FirstOrDefault(it => it.Type == System.Security.Claims.ClaimTypes.Uri);

			HyperStore = await NetStore.ConnectAsyncThrows(claimConnection.Value);

			HyperStore.DefaultAuthenticationInfo = new HyperAuthenticationInfo()
			{
				Auth = new HyperArgsAuthentication()
				{
					Token = claimToken.Value
				}
			};

			try
			{
				// Verify connection is correct.
				await HyperStore.ExecuteAsyncThrows(new RetrieveAssetsIdsArgs() { Limit = 1 });
			}
			catch (Exception ex)
			{// Handle failed login here.
			}

			await base.OnInitializedAsync();
		}

		protected virtual void OnToastMessage(ToastMessage msg)
		{
			ToastRef.Show(msg);
		}
	}
}
