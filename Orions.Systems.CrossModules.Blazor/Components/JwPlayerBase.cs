using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

namespace Orions.Systems.CrossModules.Blazor.Components
{
	public class JwPlayerBase : BaseOrionsComponent
	{
		protected JwPlayerConfig JwPlayerConfig { get; set; } = new JwPlayerConfig();


		/// <summary>
		/// Unique element ID
		/// </summary>
		[Parameter]
		public string Id
		{
			get
			{
				return JwPlayerConfig.Id;
			}
			set
			{
				JwPlayerConfig.Id = value;
			}
		}

		/// <summary>
		/// The title of your video or audio item
		/// </summary>
		[Parameter]
		public string Title
		{
			get
			{
				return JwPlayerConfig.Title;
			}
			set
			{
				JwPlayerConfig.Title = value;
			}
		}

		/// <summary>
		/// (Required) URL to a single video file, audio file, YouTube video or live stream to play.
		/// Can also be configured inside of a<see cref="Sources"/>> array
		/// </summary>
		[Parameter]
		public string File
		{
			get
			{
				return JwPlayerConfig.File;
			}
			set
			{
				JwPlayerConfig.File = value;
			}
		}

		/// <summary>
		/// URL to a poster image to display before playback starts.
		/// </summary>
		[Parameter]
		public string Image
		{
			get
			{
				return JwPlayerConfig.Image;
			}
			set
			{
				JwPlayerConfig.Image = value;
			}
		}
		/// <summary>
		/// A description of your video or audio item
		/// </summary>
		[Parameter]
		public string Description
		{
			get
			{
				return JwPlayerConfig.Description;
			}
			set
			{
				JwPlayerConfig.Description = value;
			}
		}

		/// <summary>
		/// Configures if the player should be muted during playback
		/// </summary>
		[Parameter]
		public bool Mute
		{
			get
			{
				return JwPlayerConfig.Mute;
			}
			set
			{
				JwPlayerConfig.Mute = value;
			}
		}

		/// <summary>
		/// Whether the player will attempt to begin playback automatically when a page is loaded.
		/// Set to 'viewable' to have player autostart if 50% is viewable.
		/// </summary>
		[Parameter]
		public bool Autostart
		{
			get
			{
				return JwPlayerConfig.Autostart;
			}
			set
			{
				JwPlayerConfig.Autostart = value;
			}
		}

		/// <summary>
		/// Custom text to display in the right-click menu
		/// </summary>
		[Parameter]
		public string AboutText
		{
			get
			{
				return JwPlayerConfig.AboutText;
			}
			set
			{
				JwPlayerConfig.AboutText = value;
			}
		}

		/// <summary>
		/// Custom URL to link to when clicking the right-click menu
		/// </summary>
		[Parameter]
		public string AboutLink
		{
			get
			{
				return JwPlayerConfig.AboutLink;
			}
			set
			{
				JwPlayerConfig.AboutLink = value;
			}
		}

		/// <summary>
		/// Whether to display a button in the controlbar to adjust playback speed.
		/// If true, the pre-defined options available in the menu are 0.5x, 1x, 1.25x, 1.5x, and 2x.
		/// Instead of true, an array can be passed to customize the menu options.
		/// For example: "playbackRateControls": [0.25, 0.75, 1, 1.25].
		/// </summary>
		[Parameter]
		public bool PlaybackRateControls
		{
			get
			{
				return JwPlayerConfig.PlaybackRateControls;
			}
			set
			{
				JwPlayerConfig.PlaybackRateControls = value;
			}
		}

		/// <summary>
		/// Maintains proportions when width is a percentage. Will not be used if the player is a static size.
		/// Note: Must be entered in ratio "x:y" format
		/// </summary>
		[Parameter]
		public string AspectRatio
		{
			get
			{
				return JwPlayerConfig.AspectRatio;
			}
			set
			{
				JwPlayerConfig.AspectRatio = value;
			}
		}

		/// <summary>
		/// The desired width of your video player (In pixels or percentage)
		/// </summary>
		[Parameter]
		public string Width
		{
			get
			{
				return JwPlayerConfig.Width;
			}
			set
			{
				JwPlayerConfig.Width = value;
			}
		}

		/// <summary>
		/// The desired height of your video player (In pixels). Can be omitted when aspectratio is configured
		/// </summary>
		//[Parameter]
		//public int Height
		//{
		//    get
		//    {
		//        return JwPlayerConfig.Height;
		//    }
		//    set
		//    {
		//        JwPlayerConfig.Height = value;
		//    }
		//}

		/// <summary>
		/// Configures if the title of a media file should be displayed
		/// </summary>
		[Parameter]
		public bool DisplayTitle
		{
			get
			{
				return JwPlayerConfig.DisplayTitle;
			}
			set
			{
				JwPlayerConfig.DisplayTitle = value;
			}
		}
		/// <summary>
		/// Player Logo File
		/// </summary>
		[Parameter]
		public string LogoFile
		{
			get
			{
				return JwPlayerConfig.LogoConfig.File;
			}
			set
			{
				JwPlayerConfig.LogoConfig.File = value;
			}
		}

		/// <summary>
		/// List of video/audio Sources
		/// </summary>
		[Parameter]
		public List<PlayerSource> Sources { get; set; } = new List<PlayerSource>();


		protected override async Task OnFirstAfterRenderAsync()
		{

			if (JwPlayerConfig != null)
			{
				if (Sources != null)
				{
					JwPlayerConfig.Sources = Sources;
				}

				await JsInterop.InvokeAsync<object>("Orions.JwPlayer.init", new object[] { JwPlayerConfig });
			}
		}

		public async Task RemovePlayer(string id)
		{
			await JsInterop.InvokeAsync<object>("Orions.JwPlayer.remove", new object[] { id });
		}
	}
}
