using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.Tagging;
using Orions.Systems.Desi.Common.TagsExploitation;
using Orions.Systems.Desi.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.TaskExploitation;
using Orions.Systems.Desi.Core.General;
using Orions.Systems.Desi.Common.Extensions;
using System.Reactive.Linq;

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
					value.DataChanged.Subscribe(_ => UpdateState());
					value.Data?.GetPropertyChangedObservable()
						.Where(i => i.EventArgs.PropertyName == nameof(TagsExploitationData.CurrentTaskTags))
						.Subscribe(_ => {
								value.Data?.CurrentTaskTags.GetCollectionChangedObservable()
									.Subscribe(e => 
									{
										e.EventArgs.NewItems.Foreach(t =>
										{
											(t as TagModel).GetPropertyChangedObservable().Subscribe(_ => UpdateState());
										});

										UpdateState();
									});
								UpdateState();
							});

					value.Data?.CurrentTaskTags.GetCollectionChangedObservable()
						.Subscribe(_ => UpdateState());

					value.Data?.CurrentTaskTags.Foreach(t =>
					{
						t.GetPropertyChangedObservable()
							.Subscribe(_ => UpdateState());
					});
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
				SetProperty(ref _taskDataStore, value, () =>
				{

					_taskDataStore = value;
				});
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

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			
		}
	}
}
