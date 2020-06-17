using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Core.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Tagging;
using Orions.Systems.CrossModules.Desi.Components.TaggingSurface;
using System.Reactive.Linq;
using System.Collections.Generic;
using Orions.Systems.CrossModules.Desi.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.CrossModules.Components.Desi.Services;
using Orions.Systems.Desi.Common.Util;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.CrossModules.Desi.Components.TabSet;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class TaggingPageBase : BaseViewModelComponent<TaggingViewModel>
	{
		protected override bool AutoWirePropertyChangedListener => false;
		protected TaggingSurface TaggingSurface;

		[Inject]
		public IKeyboardListener KeyboardListener { get; set; }

		[Inject]
		public INavigationService NavigationService { get; set; }

		[Inject]
		public ITaggingSystem TaggingSystem { get; set; }

		[Inject]
		public IAuthenticationSystem AuthenticationSystem { get; set; }

		[Inject]
		public TaggingViewModel AuthenticationViewModel
		{
			get
			{
				return Vm;
			}
			set
			{
				Vm = value;
			}
		}

		protected Tab TrackingTab { get; set; }
		protected Tab TagsTab { get; set; }

		protected override async Task OnInitializedAsyncSafe()
		{
			_dataStoreSubscriptions.Add(TaggingSystem.TaskDataStore.CurrentTaskChanged.Subscribe(_ => this.UpdateState()));
			_dataStoreSubscriptions.Add(TaggingSystem.TaskDataStore.CurrentTaskExpandedChanged.Subscribe(_ => this.UpdateState()));
			_dataStoreSubscriptions.Add(TaggingSystem.TagsStore.SelectedTagsUpdated.Subscribe(tags => this.OnSelectedTagsUpdated(tags)));
			_dataStoreSubscriptions.Add(TaggingSystem.TrackingDataStore.TrackingSequenceCreatedForTag.Subscribe(_ => this.TagsTab.Activate()));
			_dataStoreSubscriptions.Add(KeyboardListener.CreateSubscription()
				.AddShortcut(Key.T, () => Vm.ActivateTagonomyExecutionCommand.Execute(null))
				.AddShortcut(Key.N, () => Vm.ConfirmCurrentTaskTagsCommand.Execute(null))
				.AddShortcut(Key.P, () => Vm.GoPreviousTaskCommand.Execute(null))
				.AddShortcut(Key.Delete, () => {
					if (Vm.TagData.SelectedTags.Any()) 
						Vm.RemoveTagCommand.Execute(Vm.TagData.SelectedTags.First());
				}));
		}

		private void OnSelectedTagsUpdated(IReadOnlyCollection<TagModel> tags)
		{
			if ((Vm.CurrentTask?.HasCreateTrackingSequence() ?? false) && tags.All(i => i.TrackingSequence == null))
			{
				TrackingTab.Activate();
			}
		}

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
				Vm?.Dispose();
				Vm = null;
			}
			base.Dispose(disposing);
		}
	}
}
