using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class DashboardWidget : IdUnifiedBlob, IDashboardWidget
	{
		public string Label { get { return TitleSettings.Title; } set { TitleSettings.Title = value; } }

		public bool ShowTitle { get { return TitleSettings.IsShow; } set { TitleSettings.IsShow = value; } }

		public bool ShowFooter { get; set; } = false;

		public bool IsVisibile { get; set; } = true;

		public TitleConfiguration TitleSettings { get; set; } = new TitleConfiguration();

		[HelpText("Top separator options")]
		public SeparatorConfiguration TopSeparator { get; set; } = new SeparatorConfiguration();

		[HelpText("Bottom separator options")]
		public SeparatorConfiguration BottomSeparator { get; set; } = new SeparatorConfiguration();

		[HelpText("Group of filters this widget belongs to")]
		public string FilterGroup { get; set; }

		//public ReportInstruction.Targets FilterGroupTarget { get; set; } = ReportInstruction.Targets.Default;

		public DashboardWidget()
		{
			this.Id = IdGeneratorHelper.Generate("widget-");
		}

		protected virtual string PrintLabel()
		{
			return $"Widget : {Label}";
		}
	}
}
