using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Media;
using Orions.Systems.Desi.Common.General;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface.Media
{
	public class MediaComponentBase: BaseComponent
	{
		private MediaInstance _media;

		[Parameter]
		public MediaInstance Media
		{
			get => _media;
			set => SetProperty(ref _media, value, UpdateState);
		}

		[Parameter]
		public IActionDispatcher ActionDispatcher { get; set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
			base.Dispose(disposing);
		}
	}
}
