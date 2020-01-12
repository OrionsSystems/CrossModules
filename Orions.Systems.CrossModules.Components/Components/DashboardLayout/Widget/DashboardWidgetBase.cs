using Orions.Common;
using Orions.Node.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class DashboardWidgetBase : IdUnifiedBlob, IDashboardWidget 
	{
		public string Label { get; set; }

		public bool ShowTitle { get; set; } = true;

		public bool ShowFooter { get; set; } = true;

		public DashboardWidgetBase()
		{
			this.Id = IdGeneratorHelper.Generate("widget-");
		}

		protected virtual string PrintLabel()
		{
			return $"Widget : {Label}";
		}
	}
}
