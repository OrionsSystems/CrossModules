﻿using Orions.Systems.Desi.Common.TagsExploitation;
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
using Orions.Systems.CrossModules.Desi.Components.TaggingSurface;
using System.Reactive.Linq;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class TaggingPageBase : DesiBaseComponent<TaggingViewModel>
	{
		protected readonly List<IDisposable> _subscriptions = new List<IDisposable>();

		protected TaggingSystem _taggingSystem;
		protected TaggingSurface TaggingSurface;

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

			if(Vm.CurrentTask != null)
			{
				Vm.CurrentPosition = Vm.CurrentTask.HyperId;
			}
			_subscriptions.Add(Vm.TasksData.CurrentTaskChanged.Where(i => i.NewTask != null).Subscribe(i =>
			{
				Vm.IsTaggingMode = true;
				Vm.CurrentPosition = i.NewTask.HyperId;
			}));

			await base.OnInitializedAsync();
		}

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

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (this.Vm?.TagonomyExecutionData != null)
			{
				await TaggingSurface.AttachElementPositionToRectangle(Vm.TagData.CurrentTaskTags.Single(t => t.IsSelected).Id.ToString(), ".vizlist-positioned");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_subscriptions.ForEach(i => i.Dispose());
				_subscriptions.Clear();
				Vm.Dispose();
				Vm = null;
			}
			base.Dispose(disposing);
		}
	}
}
