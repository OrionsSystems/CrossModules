using Orions.Systems.Desi.Common.TagsExploitation;
using Orions.Systems.Desi.Common.Models;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Tagging;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class TaggingPageBase : DesiBaseComponent<TaggingViewModel>
	{
		protected TaggingSystem _taggingSystem;

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

			this.Vm.PropertyChanged += (s, e) =>
			{
				this.Vm.CurrentPosition = this.Vm.CurrentTask.HyperId;
			};

			await base.OnInitializedAsync();
		}

		//public async Task GetDebugImage()
		//{
		//	this.Vm.PropertyChanged += (s1, e1) =>
		//	{
		//		if (Vm.CurrentTask != null && SurfaceImageData == null)
		//		{
		//			var netstoreProvider = DependencyResolver.GetNetStoreProvider();
		//			var netStore = netstoreProvider.CurrentNetStore;
		//			var hyperId = this.Vm.CurrentTask.HyperId;

		//			var args = new RetrieveFragmentFramesArgs
		//			{
		//				AssetId = hyperId.AssetId.Value,
		//				TrackId = hyperId.TrackId.Value,
		//				FragmentId = hyperId.FragmentId.Value,
		//				SliceIds = new[] { hyperId.SliceId.Value }
		//			};
		//			netStore.ExecuteAsyncThrows(args).ContinueWith(t =>
		//			{
		//				SurfaceImageData = t.Result[0].Image.Data;
		//			});

		//		}
		//	};
		//}

		public void TagAdded(Components.TaggingSurface.Model.Rectangle rectangle)
		{
			var actionDispatcher = _taggingSystem.ActionDispatcher;

			var rectF = new System.Drawing.RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

			var tagsGeometry = new TagGeometry(rectF, ShapeType.Rectangle);
			var currentPosition = Vm.CurrentTask.HyperId;
			actionDispatcher.Dispatch(CreateNewTagAction.Create(tagsGeometry, currentPosition));
		}

		public void TagSelected(string id)
		{
			var actionDispatcher = _taggingSystem.ActionDispatcher;

			var tagModel = this.Vm.TagData.CurrentTaskTags.Single(t => t.Id.ToString() == id);
			actionDispatcher.Dispatch(ToggleTagSelectionAction.Create(tagModel));
		}
	}
}
