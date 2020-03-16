using Microsoft.AspNetCore.Components;
using Orions.Common;
using Orions.Desi.Forms.Core.Services;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Desi.Debug.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Common.MissionsExploitation;
using Orions.Systems.Desi.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Debug.Pages
{
	public class MissionsPageBase : ComponentBase
	{
		private MissionsViewModel _vm;

		public MissionsViewModel Vm
		{
			get
			{
				return _vm;
			}
			set
			{
				_vm = value;
				if (value != null)
				{
					_vm.PropertyChanged += (s, e) => this.InvokeAsync(() => this.StateHasChanged());
				}
			}
		}

		[Inject]
		public BlazorDependencyResolver DependencyResolver { get; set; }

		public MissionsPageBase()
		{
		}

		public async Task Initialize()
		{
			var authSystem = DependencyResolver.GetAuthenticationSystem();
			var authInfo = authSystem.Store.Data.AuthenticationInfo as HyperDomainAuthenticationInfo;
			authInfo.Username = "andrei";
			authInfo.Password.Value = "a9090xxx";

			authSystem.Store.Data.PropertyChanged += (s, e) => 
			{
				if(this.Vm == null && e.PropertyName == nameof(authSystem.Store.Data.AuthenticationStatus) && authSystem.Store.Data.AuthenticationStatus == AuthenticationStatus.LoggedIn)
				{
					var missionSystem = DependencyResolver.GetMissionsExploitationSystem();
					this.Vm = new MissionsViewModel(
							null,
							DependencyResolver.GetApiHelper(),
							DependencyResolver.GetDialogService(),
							DependencyResolver.GetAuthenticationSystem(),
							missionSystem
						);

					this.Vm.MissionsData.PropertyChanged += (s, e) => {
						this.InvokeAsync(() => this.StateHasChanged());
						this.Vm.MissionsData.Workflows.ForEach(wf => wf.PropertyChanged += (s, e) => this.InvokeAsync(() => this.StateHasChanged()));
					};

					this.Vm.FetchMissionsCommand?.Execute(null);
				}
			};
			authSystem.Controller.Dispatch(LoginAction.Create());
		}
	}
}
