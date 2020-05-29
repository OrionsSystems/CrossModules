using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

namespace Orions.Systems.CrossModules.Components
{
	public class ShakaPlayerBase : BaseOrionsComponent
	{
		protected ShakaPlayerConfig ShakaPlayerConfig { get; set; } = new ShakaPlayerConfig();

		/// <summary>
		/// Unique element ID
		/// </summary>
		[Parameter]
		public override string Id
		{
			get
			{
				return ShakaPlayerConfig.Id;
			}
			set
			{
				ShakaPlayerConfig.Id = value;
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
				return ShakaPlayerConfig.File;
			}
			set
			{
				ShakaPlayerConfig.File = value;
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
				return ShakaPlayerConfig.Autostart;
			}
			set
			{
				ShakaPlayerConfig.Autostart = value;
			}
		}

        [Parameter]
        public int StartAt
        {
            get
            {
                return ShakaPlayerConfig.StartAt;
            }
            set
            {
                ShakaPlayerConfig.StartAt = value;
            }
        }

		protected override async Task OnFirstAfterRenderAsync()
		{
			if (ShakaPlayerConfig == null || string.IsNullOrWhiteSpace(ShakaPlayerConfig.Id)) throw new ArgumentException(nameof(ShakaPlayerConfig));

			await JsInterop.InvokeAsync<object>("Orions.ShakaPlayer.init", new object[] { ShakaPlayerConfig });
		}

		public async Task RemovePlayer(string id)
		{
			await JsInterop.InvokeAsync<object>("Orions.ShakaPlayer.remove", new object[] { id });
		}
	}
}
