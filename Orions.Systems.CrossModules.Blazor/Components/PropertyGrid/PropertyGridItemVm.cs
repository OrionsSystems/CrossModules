using Orions.Common;
using Orions.SDK;

namespace Orions.Systems.CrossModules.Blazor.Components.PropertyGrid
{
	public class PropertyGridItemVm : BlazorVm
	{
		public PropertyGridUtility Utility { get; set; }

		public ViewModelProperty<PropertyGridItemUtilityData> DataProp { get; private set; } = new ViewModelProperty<PropertyGridItemUtilityData>();

		public DefaultCommand CustomExpandCommand { get; private set; } = new DefaultCommand();

		public PropertyGridItemVm()
		{
			//CustomExpandCommand.Owner = this;
		}

		public string GroupName { get; set; }

		public override string ToString()
		{
			return DataProp?.Value.ToString();
		}
	}
}
