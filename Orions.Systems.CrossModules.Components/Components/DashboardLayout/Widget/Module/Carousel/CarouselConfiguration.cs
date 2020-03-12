using System;
using System.Collections.Generic;

using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class CarouselConfiguration
	{
		public enum PauseFilter
		{
			False,
			Hover
		}

		[HelpText("Makes a slide clickable")]
		public bool IsActionLinkEnabled { get; set; }

		[HelpText("Add captions to your slides")]
		public bool ShowCaption { get; set; }

		[HelpText("Add Header captions to your slides")]
		public bool ShowCaptionHeader { get; set; }

		[HelpText("Animate slides with a fade transition instead of a slide")]
		public bool Fade { get; set; }


		[HelpText("The amount of time to delay between automatically cycling an item.")]
		public string Interval { get; set; } = "5000";

		//[HelpText("Whether the carousel should react to keyboard events.")]
		//public bool Keyboard { get; set; }

		[HelpText("If set to hover, pauses the cycling of the carousel on mouseenter and resumes the cycling of the carousel on mouseleave. If set to false, hovering over the carousel won't pause it")]
		public PauseFilter Pause { get; set; } = PauseFilter.False;

		[HelpText("Specifies whether the carousel should go through all slides continuously, or stop at the last slide. true - cycle continuously, false - stop at the last item")]
		public bool Wrap { get; set; } = true;

		[HelpText("Whether the carousel should support left/right swipe interactions on touchscreen devices.")]
		public bool Touch { get; set; } = true;

		public class NavigationItem
		{
			public string View { get; set; }

			[UniBrowsable(false)]
			public string ImageAsBase64 => $"data:image/jpg;base64, {Convert.ToBase64String(ImageBinary)}";

			[UniBrowsable(AdvancedType = UniBrowsableAttribute.AdvancedTypes.Mandatory, AllowCreate = false, Browsable = true, EditType = UniBrowsableAttribute.EditTypes.ImageFile)]
			public byte[] ImageBinary { get; set; }
		}

		[HelpText("Navigation items")]
		public List<NavigationItem> Navigations { get; set; } = new List<NavigationItem>();

		public CarouselConfiguration()
		{
		}
	}
}
