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

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class TaggingPageBase : DesiBaseComponent<TaggingViewModel>
	{
		private TaggingSystem _taggingSystem;

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

			// debug 
			await GetDebugImage();

			await base.OnInitializedAsync();
		}

		// debug
		public byte[] DebugImageData { get; set; }

		public async Task GetDebugImage()
		{
			var hyperId = HyperId.Parse("/45612d57-b029-472a-ebbc-b30564c9d708;unified.ods/1-Video+H264/948/0/");
			var netstoreProvider = DependencyResolver.GetNetStoreProvider();
			var netStore = netstoreProvider.CurrentNetStore;

			var args = new RetrieveFragmentFramesArgs
			{
				AssetId = hyperId.AssetId.Value,
				TrackId = hyperId.TrackId.Value,
				FragmentId = hyperId.FragmentId.Value,
				SliceIds = new[] { hyperId.SliceId.Value },
				//ImageQuality = imageQuality.GetValueOrDefault()
			};
			var sliceResult = await netStore.ExecuteAsyncThrows(args);

			DebugImageData = sliceResult[0].Image.Data;
		}

		public void TagAdded(Components.TaggingSurface.Model.Rectangle rectangle)
		{
			var actionDispatcher = _taggingSystem.ActionDispatcher;

			var rectF = new System.Drawing.RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

			var tagsGeometry = new TagGeometry(rectF, ShapeType.Rectangle);
			var currentPosition = HyperId.Parse("/45612d57-b029-472a-ebbc-b30564c9d708;unified.ods/1-Video+H264/948/0/");
			actionDispatcher.Dispatch(CreateNewTagAction.Create(tagsGeometry, currentPosition));
		}
	}
}
