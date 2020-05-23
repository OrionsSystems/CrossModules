using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Components.ConfirmationPopup;
using Orions.Systems.CrossModules.Desi.Components.Popper;
using Orions.Systems.CrossModules.Desi.Components.SessionIsOverPopup;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.CrossModules.Desi.Services;
using Orions.Systems.CrossModules.Desi.Util;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.Components.Desi.Services;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;
using Microsoft.JSInterop;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Common.Services;

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

		[Inject]
		public INavigationService NavigationService { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		protected bool LoginPageVisited = false;

		public bool IsSettingsInitialized { get; private set; }

		protected override async Task OnInitializedAsyncSafe()
		{
			var settingsStorage = DependencyResolver.GetSettingsStorage() as BrowserLocalSettingsStorage;

			await settingsStorage.Load();

			IsSettingsInitialized = true;

			var platform = new BlazorPlatform(DependencyResolver.GetInputHelper());
			System.Platform.Instance = platform;

			NavigationManager.NavigateTo("/");
			NavigationManager.LocationChanged += (s, e) =>
			{
				if (NavigationManager.ToBaseRelativePath(e.Location) == string.Empty)
				{
					LoginPageVisited = true;
				}

				UpdateState();
			};
		}

		// popup service initialization is performed after first app render, because required components are not initialized when OnInitializedAsyncSafe is called
		protected override void OnAfterRenderSafe(bool firstRender)
		{
			base.OnAfterRenderSafe(firstRender);

			if (firstRender)
			{
				if (PopupService is PopupService popupService)
				{
					popupService.Init(ConfirmationPopupComponent, PopperServiceComponent, SessionIsOverPopup);
				}

				JSRuntime.InvokeVoidAsync("Orions.DesiApp.initialize");
			}
		}
	}
}
