namespace Orions.Systems.CrossModules.Components
{
	public abstract class DashboardWidgetBase : IDashboardWidget
	{
		public abstract string Label { get; set; }

		protected virtual string PrintLabel() {
			return $"Widget : {Label}";
		}
	}
}
