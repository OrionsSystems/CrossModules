using Microsoft.AspNetCore.Components;
using Orions.Desi.Forms.Core.Services;
using Orions.Systems.CrossModules.Desi.Components.ConfirmationPopup;
using Orions.Systems.CrossModules.Desi.Components.Popper;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.CrossModules.Desi.Services;
using Orions.Systems.CrossModules.Desi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi
{
	public class AppComponentBase : BaseComponent
	{
		protected ConfirmationPopupBase ConfirmationPopupComponent;
		protected PopperServiceComponentBase PopperServiceComponent;

		[Inject]
		public BlazorDependencyResolver DependencyResolver { get; set; }
		public bool IsSettingsInitialized { get; private set; }

		protected override async Task OnInitializedAsyncSafe()
		{
			var settingsStorage = DependencyResolver.GetSettingsStorage() as BrowserLocalSettingsStorage;

			await settingsStorage.Load();

			IsSettingsInitialized = true;

			var platform = new BlazorPlatform(DependencyResolver.GetInputHelper());
			System.Platform.Instance = platform;
		}

		protected override Task OnAfterRenderAsyncSafe(bool firstRender)
		{
			if (firstRender)
			{
				var popupService = DependencyResolver.GetPopupService();
				popupService.ConfirmationPopupComponent = ConfirmationPopupComponent;
				popupService.PopperServiceComponent = PopperServiceComponent;
			}

			return Task.CompletedTask;
		}
	}
}
