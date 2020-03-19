using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.Services;

namespace Orions.Systems.CrossModules.Desi.Infrastructure
{
	public class NavigationService : INavigationService
	{
		private NavigationManager _navManager;

		public NavigationService(NavigationManager navManager)
		{
			_navManager = navManager;
		}

		public Task GoBack(bool? useModalNavigation = null, bool animated = true) => throw new NotImplementedException();
		public Task GoBackFrom(object context, bool animated = true) => throw new NotImplementedException();
		public Task GoBackFromTaggingPage() => throw new NotImplementedException();
		public Task GoToLoginPage()
		{
			_navManager.NavigateTo("/", true);
			return Task.CompletedTask;
		}
		public Task GoToManageCustomNodes() => throw new NotImplementedException();

		public Task GoToMissionsPage()
		{
			_navManager.NavigateTo("missions");
			return Task.CompletedTask;
		}

		public Task GoToStageSelection()
		{
			_navManager.NavigateTo("stage-selection");

			return Task.CompletedTask;
		}

		public Task GoToTaggingPage()
		{
			_navManager.NavigateTo("tagging");

			return Task.CompletedTask;
		}
		public Task<bool> GoToTagsActionConfirmation(IEnumerable<TagModel> tagModel) => throw new NotImplementedException();
		public Task<bool> ShowSessionIsOverPopup(TimeSpan timeout) => throw new NotImplementedException();
	}
}
