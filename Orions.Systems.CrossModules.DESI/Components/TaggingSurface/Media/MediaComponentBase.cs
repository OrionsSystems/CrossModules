using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Media;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.TaskExploitation;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TagsExploitation;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface.Media
{
	public class MediaComponentBase: BaseComponent
	{
		private MediaInstance _media;

		[Parameter]
		public EventCallback OnMediaPaused { get; set; }

		[Parameter]
		public EventCallback OnMediaPlay { get; set; }

		[Parameter]
		public EventCallback OnMediaLoading { get; set; }

		[Parameter]
		public EventCallback OnMediaLoaded { get; set; }

		[Parameter]
		public EventCallback<TagModel> OnTagSelected { get; set; }

		[Parameter]
		public MediaInstance Media
		{
			get => _media;
			set => SetProperty(ref _media, value, UpdateState);
		}

		[Inject]
		public IActionDispatcher ActionDispatcher { get; set; }

		[Inject]
		public ITaskDataStore TaskDataStore { get; set; }

		[Inject]
		public ITagsStore TagsStore { get; set; }

		public TaskModel CurrentTask => TaskDataStore.Data?.CurrentTask;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
			base.Dispose(disposing);
		}
	}
}
