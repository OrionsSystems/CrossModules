using Orions.Common;
using Orions.Infrastructure.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class PropertyGridItemVm : BlazorVm
	{
		public PropertyGridUtility Utility { get; set; }

		public ViewModelProperty<PropertyGridItemUtilityData> DataProp { get; private set; } = new ViewModelProperty<PropertyGridItemUtilityData>();

		public BlazorCommand CustomExpandCommand { get; private set; } = new BlazorCommand();

		public BlazorCommand OpenImageCommand { get; private set; } = new BlazorCommand();

		public PropertyGridItemVm()
		{
			OpenImageCommand.Delegate = OnOpenImage;
			//CustomExpandCommand.Owner = this;
		}

		public string GroupName { get; set; }

		public override string ToString()
		{
			return DataProp?.Value.ToString();
		}

		private void OnOpenImage(DefaultCommand command, object parameter)
		{
			
		}
	}
}
