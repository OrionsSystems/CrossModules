using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Core.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Tagging;
using Orions.Systems.CrossModules.Desi.Components.TaggingSurface;
using System.Reactive.Linq;
using Orions.Systems.CrossModules.Desi.Components.SessionIsOverPopup;
using System.Collections.Generic;
using Orions.Systems.Desi.Common.TagsExploitation;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class TaggingPageBase : DesiBaseComponent<TaggingViewModel>
	{
		protected TaggingSystem _taggingSystem;
		private List<IDisposable> _subscriptions = new List<IDisposable>();
		protected TaggingSurface TaggingSurface;
		private SessionIsOverPopup _sessionIsOverPopup;

		public SessionIsOverPopup SessionIsOverPopup
		{
			get { return _sessionIsOverPopup; }
			set 
			{ 
				_sessionIsOverPopup = value;
				var popupService = DependencyResolver.GetPopupService();
				popupService.SessionIsOverPopup = _sessionIsOverPopup;
			}
		}

		protected override async Task OnInitializedAsync()
		{
			var navigationService = DependencyResolver.GetNavigationService();
			_taggingSystem = DependencyResolver.GetTaggingSystem(navigationService);
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
				DependencyResolver.GetFrameCacheService(),
				DependencyResolver.GetClipboardService(),
				DependencyResolver.GetNetStoreProvider(),
				_taggingSystem,
				DependencyResolver.GetDispatcher(),
				DependencyResolver.GetLoggerService(),
				DependencyResolver.GetDeviceClipboardService());

			_taggingSystem.TagsStore.SelectedTagsUpdated.Subscribe(_ => UpdateVizListPosition());

			_subscriptions.Add(
				_taggingSystem.TaskDataStore.CurrentTaskChanged.Subscribe(_ => this.UpdateState())
				);

			await base.OnInitializedAsync();
		}

		protected override bool AutoWirePropertyChangedListener => true;

		private void UpdateVizListPosition() 
		{
			if (Vm?.TagData != null && Vm.TagData.SelectedTags.Any())
			{
				StateHasChanged();
				TaggingSurface?.AttachElementPositionToRectangle(Vm.TagData.SelectedTags.First().Id.ToString(), ".vizlist-positioned");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach(var sub in _subscriptions)
				{
					sub.Dispose();
				}
				Vm?.Dispose();
				Vm = null;
			}
			base.Dispose(disposing);
		}
	}
}
