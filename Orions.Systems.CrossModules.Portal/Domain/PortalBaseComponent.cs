using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.Components;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Microsoft.AspNetCore.Authorization;
using Orions.Infrastructure.HyperMedia.Formats.Mpeg;
using Microsoft.AspNetCore.Components.Authorization;
using Orions.Systems.CrossModules.Portal.Providers;

namespace Orions.Systems.CrossModules.Portal.Domain
{
	[Authorize]
	public class PortalBaseComponent : BaseOrionsComponent
	{
		public PortalBaseComponent()
		{

		}

		protected override async Task OnInitializedAsync()
		{
			await InitHyperStoreInstanceAsync();
			await base.OnInitializedAsync();
		}

		protected async Task InitHyperStoreInstanceAsync()
		{
			if (HyperStore == null)
				HyperStore = await NetStore.ConnectAsyncThrows("http://usnbods01wan.orionscloud.com:8600/Execute");
		}

		public async Task AuthenticateAsync(CustomAuthenticationStateProvider provider)
		{
			await InitHyperStoreInstanceAsync();				

			var authState = await provider.GetAuthenticationStateAsync();

			var claim = authState.User.Claims.FirstOrDefault(it => it.Type == System.Security.Claims.ClaimTypes.Sid);

			HyperStore.DefaultAuthenticationInfo = new HyperAuthenticationInfo()
			{
				Auth = new HyperArgsAuthentication()
				{
					Token = claim.Value
				}
			};
		}

		public IHyperArgsSink HyperStore { get; set; }
	}
}
