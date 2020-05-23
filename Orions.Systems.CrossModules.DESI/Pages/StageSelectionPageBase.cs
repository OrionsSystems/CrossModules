using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.Desi.Core.ViewModels;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class StageSelectionPageBase : BaseViewModelComponent<StageSelectionViewModel>
	{
		[Inject]
		public INavigationService NavigationService { get; set; }

		[Inject]
		public IAuthenticationSystem AuthenticationSystem { get; set; }

		protected override async Task OnInitializedAsyncSafe()
		{
			this.Vm.MissionsData.CurrentWorkflow.Stages = new List<PhaseModel>(); // refactor this
			this.Vm.StartUpdateMissionStages();
		}

		public void Switch_Toggled(PhaseModel phase)
		{
			phase.IsSelected = !phase.IsSelected;
			this.Vm.ValidateStageSelection();
		}
	}
}
