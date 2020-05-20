using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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

		public async Task GoBack(bool? useModalNavigation = null, bool animated = true)
		{
			await _jSRuntime.InvokeVoidAsync("history.back");
		}
		public async Task GoBackFrom(object context, bool animated = true)
		{
			await _jSRuntime.InvokeVoidAsync("history.back");
		}

		public Task GoBackFromTaggingPage()
		{
			_navManager.NavigateTo("missions");

			return Task.CompletedTask;
		}

		public Task GoToLoginPage()
		{
			_navManager.NavigateTo("/");
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
	}
}
