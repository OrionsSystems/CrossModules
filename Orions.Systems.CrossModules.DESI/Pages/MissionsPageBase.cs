using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.Desi.Core.ViewModels;
namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class MissionsPageBase : BaseViewModelComponent<MissionsViewModel>
	{
		[Inject]
		public INavigationService NavigationService { get; set; }

		[Inject]
		public IAuthenticationSystem AuthenticationSystem { get; set; }

		protected override async Task OnInitializedAsyncSafe()
		{
			await Initialize();
		}

		public async Task Initialize()
		{
			this.Vm.FetchMissionsCommand?.Execute(null);
		}
	}
}
