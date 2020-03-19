using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.Services;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class NavigationService : INavigationService
	{
		private NavigationManager _navManager;
		private readonly IJSRuntime _jSRuntime;

		public NavigationService(NavigationManager navManager, IJSRuntime jSRuntime)
		{
			_navManager = navManager;
			_jSRuntime = jSRuntime;
		}

		public Task GoBack(bool? useModalNavigation = null, bool animated = true) => throw new NotImplementedException();
		public Task GoBackFrom(object context, bool animated = true) => throw new NotImplementedException();
		public Task GoBackFromTaggingPage()
		{
			_jSRuntime.InvokeVoidAsync("history.back");

			return Task.CompletedTask;
		}
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
