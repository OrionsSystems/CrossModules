using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Media;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.TaskExploitation;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TagsExploitation;

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
		public MediaInstance Media
		{
			get => _media;
			set => SetProperty(ref _media, value, UpdateState);
		}

		[Parameter]
		public IActionDispatcher ActionDispatcher { get; set; }

		[Parameter]
		public ITaskDataStore TaskDataStore { get; set; }

		[Parameter]
		public ITagsStore TagsStore { get; set; }

		public TaskModel CurrentTask { get { return TaskDataStore?.Data?.CurrentTask; } }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
			base.Dispose(disposing);
		}
	}
}
