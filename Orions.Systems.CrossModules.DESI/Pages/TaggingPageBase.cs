using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class TaggingPageBase : DesiBaseComponent<TaggingViewModel>
	{
		protected override async Task OnInitializedAsync()
		{
			var navigationService = DependencyResolver.GetNavigationService();
			var taggingSystem = DependencyResolver.GetTaggingSystem(navigationService);
			var authSystem = DependencyResolver.GetAuthenticationSystem();

			if (authSystem.Store.Data.AuthenticationStatus == AuthenticationStatus.LoggedOut)
			{
				await navigationService.GoToLoginPage();
				await base.OnInitializedAsync();

				return;
			}

			this.Vm = new TaggingViewModel(
				DependencyResolver.GetMissionsExploitationSystem(),
				navigationService,
				DependencyResolver.GetDialogService(),
				DependencyResolver.GetImageService(),
				DependencyResolver.GetTagFactory(),
				DependencyResolver.GetFrameCacheService(),
				DependencyResolver.GetClipboardService(),
				DependencyResolver.GetNetStoreProvider(),
				taggingSystem,
				DependencyResolver.GetDispatcher(),
				DependencyResolver.GetLoggerService(),
				DependencyResolver.GetDeviceClipboardService());

			await base.OnInitializedAsync();
		}
	}
}
