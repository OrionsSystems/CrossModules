using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
	public class JwPlayerConfig
	{
		/// <summary>
		/// Unique element ID
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// The title of your video or audio item
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// (Required) URL to a single video file, audio file, YouTube video or live stream to play.
		/// Can also be configured inside of a<see cref="Sources"/>> array
		/// </summary>
		public string File { get; set; }

		/// <summary>
		/// URL to a poster image to display before playback starts.
		/// </summary>
		public string Image { get; set; }
		/// <summary>
		/// A description of your video or audio item
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Configures if the player should be muted during playback
		/// </summary>
		public bool Mute { get; set; }

		/// <summary>
		/// Whether the player will attempt to begin playback automatically when a page is loaded.
		/// Set to 'viewable' to have player autostart if 50% is viewable.
		/// </summary>
		public bool Autostart { get; set; }

		/// <summary>
		/// Custom text to display in the right-click menu
		/// </summary>
		public string AboutText { get; set; }

		/// <summary>
		/// Custom URL to link to when clicking the right-click menu
		/// </summary>
		public string AboutLink { get; set; }

		/// <summary>
		/// Whether to display a button in the controlbar to adjust playback speed.
		/// If true, the pre-defined options available in the menu are 0.5x, 1x, 1.25x, 1.5x, and 2x.
		/// Instead of true, an array can be passed to customize the menu options.
		/// For example: "playbackRateControls": [0.25, 0.75, 1, 1.25].
		/// </summary>
		public bool PlaybackRateControls { get; set; }

		/// <summary>
		/// Maintains proportions when width is a percentage. Will not be used if the player is a static size. 
		/// Note: Must be entered in ratio "x:y" format
		/// </summary>
		public string AspectRatio { get; set; } = "16:9";

		/// <summary>
		/// The desired width of your video player (In pixels or percentage)
		/// </summary>
		public string Width { get; set; } = "100%";

		/// <summary>
		/// The desired height of your video player (In pixels). Can be omitted when aspectratio is configured	
		/// </summary>
		public int Height { get; set; } 

		/// <summary>
		/// Configures if the title of a media file should be displayed
		/// </summary>
		public bool DisplayTitle { get; set; } = true;
		
		/// <summary>
		/// List of video/audio Sources
		/// </summary>
		public List<PlayerSource> Sources { get; set; } = new List<PlayerSource>();

		/// <summary>
		/// Logo Config
		/// </summary>
		public PlayerLogoConfig LogoConfig { get; set; } = new PlayerLogoConfig();

		/// <summary>
		/// Resize images and video to fit player dimensions. See graphic below for examples 
		/// "uniform": Fits JW Player dimensions while maintaining aspect ratio 
		/// "exactfit": Will fit JW Player dimensions without maintaining aspect ratio 
		/// "fill": Will zoom and crop video to fill dimensions, maintaining aspect ratio 
		/// "none": Displays the actual size of the video file. (Black borders)
		/// </summary>
		public string Stretching { get; set; } = "exactfit";

		public bool Autoplay { get; set; } = true;

		/// <summary>
		/// Tells the player if content should be loaded prior to playback. Useful for faster playback speed or if certain metadata should be loaded prior to playback: 
        /// "none": Player will explicitly not preload content 
        /// "metadata": Only basic playback information will be loaded
        /// "auto": Browser attempts to load more of the video
        /// If you are concerned about excess content usage, we suggest setting "preload":"none"
		/// </summary>
		public string Preload { get; set; } = "auto";


        /// <summary>
		/// Initial playback position to start from
		/// </summary>
        public int StartAt { get; set; }

        /// <summary>
        /// Jw player license key
        /// </summary>
        public string Key => "0cEcLfyR+RBLPh2z3KkWfFzX1U/w/2AtglS1xoT4xl8=";

	}

	public class PlayerLogoConfig
	{
		public string File { get; set; }
		public string Position { get; set; } = "top-right";
		public string Height { get; set; } = "10%";
	}

	public class PlayerSource
	{
		public string File { get; set; }
		public string Label { get; set; }
	}
}
