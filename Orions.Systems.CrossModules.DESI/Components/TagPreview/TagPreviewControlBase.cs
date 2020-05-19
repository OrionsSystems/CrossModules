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
using System.Collections.Specialized;
using Microsoft.JSInterop;
using System.Collections.Immutable;

namespace Orions.Systems.CrossModules.Desi.Components.TagPreview
{
	public class TagPreviewControlBase : BaseComponent
	{
		[Parameter]
		public IActionDispatcher ActionDispatcher { get; set; }

		private ITagsStore _tagsStore;

		[Parameter]
		public ITagsStore TagsStore
		{
			get { return _tagsStore; }
			set
			{
				SetProperty(ref _tagsStore, value, () =>
				{
					_dataStoreSubscriptions.Add(value.DataChanged.Subscribe(_ => UpdateState()));
					_dataStoreSubscriptions.Add(
						value.Data?.GetPropertyChangedObservable()
							.Where(i => i.EventArgs.PropertyName == nameof(TagsExploitationData.CurrentTaskTags))
							.Subscribe(_ =>
							{
								value.Data.CurrentTaskTags.Foreach(t =>
								{
									t.GetPropertyChangedObservable().Subscribe(_ => UpdateState());
								});

								UpdateState();
							}));


					value.Data?.CurrentTaskTags.Foreach(t =>
					{
						_dataStoreSubscriptions.Add(
							t.GetPropertyChangedObservable()
								.Subscribe(_ => UpdateState()));
					});

					_dataStoreSubscriptions.Add(
					value.Data.GetPropertyChangedObservable()
						.Where(i => i.EventArgs.PropertyName == nameof(TagsExploitationData.SelectedTags))
						.Select(i => i.Source.SelectedTags)
						.Subscribe(OnSelectedTagsCollectionChanged));
				});
			}
		}

		private ITaskDataStore _taskDataStore;

		[Parameter]
		public ITaskDataStore TaskDataStore
		{
			get { return _taskDataStore; }
			set
			{
				SetProperty(ref _taskDataStore, value, () => _taskDataStore = value);
			}
		}

		[Parameter]
		public Command EditTagCommand { get; set; }

		[Parameter]
		public Command RemoveTagCommand { get; set; }

		public TagsExploitationData TagData { get => _tagsStore.Data; }


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
