using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TagsExploitation;
using System;
using System.Linq;
using Orions.Systems.Desi.Common.TaskExploitation;
using Orions.Systems.Desi.Core.General;
using Orions.Systems.Desi.Common.Extensions;
using System.Reactive.Linq;
using Microsoft.JSInterop;
using System.Collections.Immutable;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;

namespace Orions.Systems.CrossModules.Desi.Components.TagPreview
{
	public class TagPreviewControlBase : BaseComponent
	{
		[Inject]
		public IActionDispatcher ActionDispatcher { get; set; }


		[Inject]
		public ITagsStore TagsStore { get; set; }

		[Inject]
		public ITaskDataStore TaskDataStore { get; set; }

		[Parameter]
		public Command EditTagCommand { get; set; }

		[Parameter]
		public Command RemoveTagCommand { get; set; }

		public TagsExploitationData TagData => TagsStore.Data;

		protected override void OnInitializedSafe()
		{
			base.OnInitializedSafe();

			_dataStoreSubscriptions.AddItem(TagsStore.DataChanged.Subscribe(_ => UpdateState()))
				.AddItem(TagsStore.TagPropertyChanged.Subscribe(_ => UpdateState()))
				.AddItem(TagsStore.Data.GetPropertyChangedObservable()
					.Where(i => i.EventArgs.PropertyName == nameof(TagsExploitationData.SelectedTags))
					.Select(i => i.Source.SelectedTags)
					.Subscribe(OnSelectedTagsCollectionChanged));
		}

		protected Guid? CurrentTagonomyInfoShown;

		public void SelectItem(TagModel tag)
		{
			ActionDispatcher.Dispatch(ToggleTagSelectionAction.Create(tag));
		}

		private void OnSelectedTagsCollectionChanged(ImmutableList<TagModel> tags)
		{
			if (tags?.Count > 0)
			{
				JSRuntime.InvokeVoidAsync("Orions.TagPreviewControl.scrollToTag", new object[] { tags.First().Id.ToString() }) ;
			}
		}
	}
}
