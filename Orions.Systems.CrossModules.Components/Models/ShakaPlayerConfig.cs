using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
	public class ShakaPlayerConfig
	{
		/// <summary>
		/// Unique element ID
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// (Required) URL to a single video file, audio file, YouTube video or live stream to play.
		/// Can also be configured inside of a<see cref="Sources"/>> array
		/// </summary>
		public string File { get; set; }

				/// <summary>
		/// Whether the player will attempt to begin playback automatically when a page is loaded.
		/// Set to 'viewable' to have player autostart if 50% is viewable.
		/// </summary>
		public bool Autostart { get; set; }

		/// <summary>
		/// Initial playback position to start from
		/// </summary>
        public int StartAt { get; set; }

		public bool ShowControls { get; set; } = true;
	}
}
