using Orions.Common;
using Orions.SDK;

namespace Orions.Systems.CrossModules.Blazor.Components.PropertyGrid
{
	public class PropertyGridAdvancedItemVm : BaseVm
	{
		public PropertyGridUtility Utility { get; set; }

		public ViewModelProperty<PropertyGridItemUtilityData> DataProp { get; private set; } = new ViewModelProperty<PropertyGridItemUtilityData>();

		public DefaultCommand CustomExpandCommand { get; private set; } = new DefaultCommand();

		public PropertyGridAdvancedItemVm()
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
