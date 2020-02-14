using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class TitleConfiguration
	{
		public int FontSize { get; set; } = 20;

		public bool IsUppercase { get; set; } = true;

		[HelpText("Letter spacing in px")]
		public int LetterSpacing { get; set; } = 2;

		[HelpText("Show or hide the title")]
		public bool IsShow { get; set; } = false;

		[HelpText("Add value")]
		public string Title { get; set; }

		[HelpText("Set aligment")]
		public TitleAligment Aligment { get; set; }

		[HelpText("Add styles to title")]
		public string InlineStyles { get; set; }

		[HelpText("Add border line under title")]
		public SeparatorConfiguration SepratorsSettings { get; set; } = new SeparatorConfiguration() { Height = 1 };
	}
}
