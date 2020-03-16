using Microsoft.AspNetCore.Components;
using Orions.Common;
using Orions.Desi.Forms.Core.Services;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Desi.Debug.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Debug.Pages
{
	public class MissionsPageBase : ComponentBase
	{
		public MissionsViewModel Vm { get; set; }

		public MissionsPageBase()
		{

		}

		public async Task Initialize()
		{
			var username = "andrei";
			var password = "password";

			var domainHelper = new HyperDomainHelper("andrei", "a9090xxx");
			await domainHelper.LoginToDomainAsyncThrows();

			var nodeModels = new List<HyperNodeAuthenticationInfo>();
			var nodes = await domainHelper.TryRetrieveAccessibleNodesAsync();
			foreach (var node in nodes.Result)
			{
				var connectionString = WebHelper.ConstructUserPassUri(node.ConnectionUriNoCredentials, username, password);
				var model = new HyperNodeAuthenticationInfo(connectionString, node.Alias);
				nodeModels.Add(model);
			}


			var dependencyResolver = new BlazorDependencyResolver();
			dependencyResolver.GetNetStoreProvider().SetAvailableNodes(nodeModels);

			
			var viewModel = new MissionsViewModel(null,
				dependencyResolver.GetApiHelper(),
				dependencyResolver.GetDialogService(),
				dependencyResolver.GetAuthenticationSystem(),
				dependencyResolver.GetMissionsExploitationSystem());
			this.Vm = viewModel;
		}
	}
}
