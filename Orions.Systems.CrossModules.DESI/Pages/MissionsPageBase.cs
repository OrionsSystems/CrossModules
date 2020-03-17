using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Orions.Systems.CrossModules.Desi.Debug.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Core.ViewModels;

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

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await Initialize();
			}
		}

		public async Task Initialize()
		{
			var authSystem = DependencyResolver.GetAuthenticationSystem();

			if (authSystem.Store.Data.AuthenticationStatus == AuthenticationStatus.LoggedOut)
				return;

			var settingsStorage = DependencyResolver.GetSettingsStorage();

			var missionSystem = DependencyResolver.GetMissionsExploitationSystem();
			this.Vm = new MissionsViewModel(
					null,
					DependencyResolver.GetApiHelper(),
					DependencyResolver.GetDialogService(),
					DependencyResolver.GetAuthenticationSystem(),
					missionSystem
				);

			this.Vm.MissionsData.PropertyChanged += (s, e) =>
			{
				this.InvokeAsync(() => this.StateHasChanged());
				this.Vm.MissionsData.Workflows.ForEach(wf => wf.PropertyChanged += (s, e) => this.InvokeAsync(() => this.StateHasChanged()));
			};

			this.Vm.FetchMissionsCommand?.Execute(null);
		}
	}
}
