using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public class TitleConfiguration
	{
		[HelpText("Show or hide the title")]
		public bool IsShow { get; set; } = false;

		[HelpText("Add value")]
		public string Title { get; set; }

		[HelpText("Set aligment")]
		public TitleAligment Aligment { get; set; }

		[HelpText("Add styles to title")]
		public string InlineStyles { get; set; }
	}
}
