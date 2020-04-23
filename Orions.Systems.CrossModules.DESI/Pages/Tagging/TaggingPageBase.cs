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
using Orions.Systems.CrossModules.Desi.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class TaggingPageBase : BaseViewModelComponent<TaggingViewModel>
	{
		protected TaggingSystem _taggingSystem;
		private List<IDisposable> _subscriptions = new List<IDisposable>();
		protected TaggingSurface TaggingSurface;
		private SessionIsOverPopup _sessionIsOverPopup;

		[Inject]
		public IKeyboardListener KeyboardListener { get; set; }

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

		protected override async Task OnInitializedAsyncSafe()
		{
			var navigationService = DependencyResolver.GetNavigationService();
			_taggingSystem = DependencyResolver.GetTaggingSystem(navigationService);
			var authSystem = DependencyResolver.GetAuthenticationSystem();

			if (authSystem.Store.Data.AuthenticationStatus == AuthenticationStatus.LoggedOut)
			{
				await navigationService.GoToLoginPage();

				return;
			}

			this.Vm = new TaggingViewModel(
				DependencyResolver.GetMissionsExploitationSystem(),
				navigationService,
				DependencyResolver.GetDialogService(),
				DependencyResolver.GetImageService(),
				DependencyResolver.GetClipboardService(),
				DependencyResolver.GetNetStoreProvider(),
				_taggingSystem,
				DependencyResolver.GetDispatcher(),
				DependencyResolver.GetLoggerService(),
				DependencyResolver.GetDeviceClipboardService());

			_subscriptions.Add(
				_taggingSystem.TaskDataStore.CurrentTaskChanged.Subscribe(_ => this.UpdateState())
				);

			_subscriptions.Add(KeyboardListener.CreateSubscription()
				.AddShortcut(Key.T, () => Vm.ActivateTagonomyExecutionCommand.Execute(null))
				.AddShortcut(Key.N, () => Vm.ConfirmCurrentTaskTagsCommand.Execute(null))
				.AddShortcut(Key.P, () => Vm.GoPreviousTaskCommand.Execute(null))
				.AddShortcut(Key.Delete, () => {
					if (Vm.TagData.SelectedTags.Any()) 
						Vm.RemoveTagCommand.Execute(Vm.TagData.SelectedTags.First());
				}));
		}

		protected override bool AutoWirePropertyChangedListener => false;

		private List<Action> _afterRenderTasks = new List<Action>();

		protected void OnVizListRendered()
		{
			if (Vm?.TagData?.SelectedTags?.FirstOrDefault() != null)
			{
				TaggingSurface?.AttachElementPositionToRectangle(Vm.TagData.SelectedTags.FirstOrDefault().Id.ToString(), ".vizlist-positioned");
			}
		}

		protected override void OnAfterRenderSafe(bool firstRender)
		{
			if (firstRender)
			{
				this.JSRuntime.InvokeVoidAsync("Orions.TaggingPage.init");
			}

			lock (_afterRenderTasks)
			{
				foreach(var t in _afterRenderTasks)
				{
					t.Invoke();
				}
				_afterRenderTasks.Clear();
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
