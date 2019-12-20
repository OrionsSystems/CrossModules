using System;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class DashboardWidgetBase : IDashboardWidget
	{
		public string Id { get; set; } = IdGeneratorHelper.Generate("widget-");

		public abstract string Label { get; set; }

		protected virtual string PrintLabel() {
			return $"Widget : {Label}";
		}
		
		public abstract Type GetViewComponent();
	}
}
