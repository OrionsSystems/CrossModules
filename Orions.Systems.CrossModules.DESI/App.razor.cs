using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Components.ConfirmationPopup;
using Orions.Systems.CrossModules.Desi.Components.Popper;
using Orions.Systems.CrossModules.Desi.Components.SessionIsOverPopup;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.CrossModules.Desi.Services;
using Orions.Systems.CrossModules.Desi.Util;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi
{
	public class AppComponentBase : BaseComponent
	{
		protected ConfirmationPopupBase ConfirmationPopupComponent;
		protected PopperServiceComponentBase PopperServiceComponent;
		protected SessionIsOverPopup SessionIsOverPopup;

		[Inject]
		public BlazorDependencyResolver DependencyResolver { get; set; }

		[Inject]
		public IPopupService PopupService { get; set; }

		public bool IsSettingsInitialized { get; private set; }

		protected override async Task OnInitializedAsyncSafe()
		{
			var settingsStorage = DependencyResolver.GetSettingsStorage() as BrowserLocalSettingsStorage;

			await settingsStorage.Load();

			IsSettingsInitialized = true;

			var platform = new BlazorPlatform(DependencyResolver.GetInputHelper());
			System.Platform.Instance = platform;

			if(PopupService is PopupService popupService)
			{
				popupService.Init(ConfirmationPopupComponent, PopperServiceComponent, SessionIsOverPopup);
			}
		}
	}
}
