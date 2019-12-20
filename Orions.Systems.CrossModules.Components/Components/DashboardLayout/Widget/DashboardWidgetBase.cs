using Orions.Node.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class DashboardWidgetBase : IDashboardWidget
	{
		public string Id { get; set; } = IdGeneratorHelper.Generate("widget-");

		public abstract string Label { get; set; }

		public bool ShowTitle { get; set; } = true;

		public bool ShowFooter { get; set; } = true;

		protected virtual string PrintLabel() {
			return $"Widget : {Label}";
		}
		
		public abstract Type GetViewComponent();
	}
}
