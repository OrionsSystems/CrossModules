using Orions.Common;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class DashboardWidget : IdUnifiedBlob, IDashboardWidget
	{
		public string Label { get; set; }

		public bool ShowTitle { get; set; } = true;

		public bool ShowFooter { get; set; } = true;

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
