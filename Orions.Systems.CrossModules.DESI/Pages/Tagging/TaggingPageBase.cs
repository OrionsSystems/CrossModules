using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Core.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Tagging;
using Orions.Systems.CrossModules.Desi.Components.TaggingSurface;
using System.Reactive.Linq;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class TaggingPageBase : DesiBaseComponent<TaggingViewModel>
	{
		protected TaggingSystem _taggingSystem;
		protected TaggingSurfaceBase TaggingSurface;
		private IDisposable _tagonomyExecutionStartedSub;

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

			_tagonomyExecutionStartedSub = _taggingSystem
				.TagonomyExecutionDataStore
				.TagonomyExecutionStarted
				.Subscribe(_ => UpdateVizListPosition());

			await base.OnInitializedAsync();
		}

		protected override bool AutoWirePropertyChangedListener => false;

		private void UpdateVizListPosition() => TaggingSurface.AttachElementPositionToRectangle(Vm.TagData.CurrentTaskTags.Single(t => t.IsSelected).Id.ToString(), ".vizlist-positioned");

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_tagonomyExecutionStartedSub.Dispose();
				Vm?.Dispose();
				Vm = null;
			}
			base.Dispose(disposing);
		}
	}
}
