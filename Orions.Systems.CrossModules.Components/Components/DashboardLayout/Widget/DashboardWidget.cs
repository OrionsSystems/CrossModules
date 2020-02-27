using Orions.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class DashboardWidget : IdUnifiedBlob, IDashboardWidget
	{
		[HelpText("Title name")]
		public string Label { get { return WidgetTitleSettings.Title; } set { WidgetTitleSettings.Title = value; } }

		[HelpText("Show or hide title")]
		public bool ShowLabel { get { return WidgetTitleSettings.IsShow; } set { WidgetTitleSettings.IsShow = value; } }

		[HelpText("Show or hide footer")]
		public bool ShowFooter { get; set; } = false;

		[HelpText("Show or hide widget on render")]
		public bool IsVisibile { get; set; } = true;

		[HelpText("Widget title options")]
		public TitleConfiguration WidgetTitleSettings { get; set; } = new TitleConfiguration();

		[UniJsonIgnore]
		[UniBrowsable(false)]
		public TitleConfiguration TitleSettings
		{
			get => WidgetTitleSettings;
			set => WidgetTitleSettings = value;
		}

		[HelpText("Top separator options")]
		public SeparatorConfiguration TopSeparator { get; set; } = new SeparatorConfiguration();

		[HelpText("Bottom separator options")]
		public SeparatorConfiguration BottomSeparator { get; set; } = new SeparatorConfiguration();

		[HelpText("Group of filters this widget belongs to")]
		public string FilterGroup { get; set; }

		[HelpText("Loader options")]
		public LoaderConfiguration LoaderSettings { get; set; } = new LoaderConfiguration();

		[HelpText("CSS class names")]
		public string Class { get; set; }

		//public ReportInstruction.Targets FilterGroupTarget { get; set; } = ReportInstruction.Targets.Default;

		public DashboardWidget()
		{
			this.Id = IdGeneratorHelper.Generate("widget-");
		}

		protected virtual string PrintLabel()
		{
			return $"Widget : {Label}";
		}

		public virtual string GetIcon()
		{
			return "<svg class=\"icon-block-chain\" enable-background=\"new 0 0 512 512\" viewBox=\"0 0 512 512\" xmlns=\"http://www.w3.org/2000/svg\"><g>" +
				"<path d=\"m256.002 242.913 210.412-121.43-210.412-121.483-210.416 121.483z\"/>" +
				"<path d=\"m240.949 268.986-210.415-121.429v242.96l210.415 121.483z\"/>" +
				"<path d=\"m271.056 268.986v243.014l210.41-121.483v-242.96z\"/></g></svg>";
		}
	}
}
