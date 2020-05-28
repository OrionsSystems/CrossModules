using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TagsExploitation;
using Orions.Systems.Desi.Common.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Components.TrackingDataWizard
{
	public class TrackingDataWizardBase : BaseComponent
	{
		[Inject]
		public ITrackingDataStore TrackingDataStore { get; set; }

		[Inject]
		public ITagsStore TagsStore { get; set; }

		[Inject]
		public IActionDispatcher ActionDispatcher { get; set; }

		protected bool StartSequenceButtonsEnabled
		{
			get
			{
				return this.TagsStore.Data.SelectedTags?.Count == 1;
			}
		}

		protected TagModel SelectedTag
		{
			get
			{
				return TagsStore.Data.SelectedTags?.LastOrDefault();
			}
		}

		protected TrackingType SelectedTrackingType { get { return TrackingDataStore.Data.TrackingType; } }

		protected void OnSetTrackingSequenceStartClick()
		{
			if(SelectedTag != null)
			{
				ActionDispatcher?.Dispatch(SetTrackingSequenceElementAction.Create(SelectedTag, TrackingSequenceElementType.Start));
			}
		}

		protected void OnClearTrackingSequenceStartClicked() 
		{
			ActionDispatcher?.Dispatch(SetTrackingSequenceElementAction.Create(null, TrackingSequenceElementType.Start));
		}

		protected void OnSetTrackingSequenceEndClick()
		{
			if (SelectedTag != null)
			{
				ActionDispatcher?.Dispatch(SetTrackingSequenceElementAction.Create(SelectedTag, TrackingSequenceElementType.End));
			}
		}

		protected void OnClearTrackingSequenceEndClicked()
		{
			ActionDispatcher?.Dispatch(SetTrackingSequenceElementAction.Create(null, TrackingSequenceElementType.End));
		}

		protected override async Task OnInitializedAsyncSafe()
		{
			await base.OnInitializedAsyncSafe();

			_dataStoreSubscriptions.Add(TrackingDataStore.DataChanged.Subscribe(_ => UpdateState()));
			_dataStoreSubscriptions.Add(TrackingDataStore.Data.GetPropertyChangedObservable()
				.Subscribe(_ => UpdateState()));
			_dataStoreSubscriptions.Add(TagsStore.SelectedTagsUpdated.Subscribe(_ => UpdateState()));
		}

		protected void OnTrackingTypeSelectionChanged(ChangeEventArgs e)
		{
			var type = Enum.Parse<TrackingType>(e.Value.ToString());
			ActionDispatcher.Dispatch(SetTrackingTypeAction.Create(type));
		}

		protected void OnCreateTrackingSequenceClicked() =>
			ActionDispatcher?.Dispatch(CreateTrackingSequenceAction.Create());

		protected void OnAddIntermediateElementClicked()
		{
			if (SelectedTag != null)
			{
				ActionDispatcher.Dispatch(SetTrackingSequenceElementAction.Create(SelectedTag, TrackingSequenceElementType.Intermediate));
			}
		}
	}
}
