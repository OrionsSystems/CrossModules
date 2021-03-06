﻿using Orions.Common;
using Orions.Node.Common;
using Orions.SDK;
using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Blazor.Components.PropertyGrid
{
	public class PropertyGridVm : BlazorVm
	{
		public PropertyGridUtility Utility { get; private set; }

		public AdvancedObservableCollection<PropertyGridItemVm> Items { get; private set; } = new AdvancedObservableCollection<PropertyGridItemVm>();

		public BlazorCommand HomeCommand { get; set; } = new BlazorCommand();

		public BlazorCommand BackCommand { get; set; } = new BlazorCommand();

		public ViewModelProperty<bool> BackButtonVisibleProp { get; set; } = new ViewModelProperty<bool>(false);

		public BlazorCommand CreatorCreateCommand { get; set; } = new BlazorCommand();

		public BlazorCommand CustomExpandAggregateCommand { get; set; } = new BlazorCommand();

		public ViewModelProperty<bool> IsReadOnlyProp { get; set; } = new ViewModelProperty<bool>();

		public ViewModelProperty<string> TrailProp { get; private set; } = new ViewModelProperty<string>();

		public ViewModelProperty<PropertyGridUtilityBase.CreatorItem> SelectedCreatorItemProp { get; set; } = new ViewModelProperty<PropertyGridUtilityBase.CreatorItem>();

		public object Source
		{
			get
			{
				return this.Utility?.Data?.Instance;
			}

			set
			{
				SettingSource?.Invoke(this, value);
				var t = this.Utility.SetSourceAsync(value, true);
			}
		}

		public event Action<PropertyGridVm, object> SettingSource;

		public IHyperArgsSink HyperStore
		{
			get
			{
				return Utility?.HyperStore;
			}

			set
			{
				if (Utility != null)
					Utility.HyperStore = value;
			}
		}

		public PropertyGridVm()
		{
			Utility = new PropertyGridUtility();
			Utility.PropertyChanged += Utility_PropertyChanged;

			Utility.TrailProp.ValueChanged += TrailProp_ValueChanged;

			Items.Link(Utility.Items, null, null, transformDelegate_Items);

			TrailProp.TakesFrom(Utility.TrailProp);

			CustomExpandAggregateCommand.AsyncDelegate = OnCustomExpand_Aggregate;
			BackCommand.AsyncDelegate = OnBack;
			HomeCommand.AsyncDelegate = OnHome;
			CreatorCreateCommand.AsyncDelegate = OnCreatorCreate;
		}

		private void Utility_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if ((e.PropertyName == nameof(PropertyGridUtility.CreatorMode) && Utility.CreatorMode)
				|| (e.PropertyName == nameof(PropertyGridUtility.ListMode) && Utility.ListMode))
			{
				// Auto select for creator items.

			}
		}

		private async Task OnHome(DefaultCommand command, object parameter)
		{
			await Utility.MoveBack(true);
		}

		private async Task OnBack(DefaultCommand command, object parameter)
		{
			await Utility.MoveBack();
		}
		private async Task OnCreatorCreate(DefaultCommand command, object parameter)
		{
			await Utility.RunCreatorFor(SelectedCreatorItemProp.Value);
		}

		private void TrailProp_ValueChanged(INotifyProperty<string> prop, string newValue)
		{
			BackButtonVisibleProp.Value = Utility.HistoryInstancesCount > 1;
		}

		private async Task OnCustomExpand_Aggregate(DefaultCommand command, object parameter)
		{
			var itemVm = parameter as PropertyGridItemVm;
			if (itemVm != null)
				await Utility.SetMainDataAdvanced(itemVm.DataProp.Value, true);
		}

		private PropertyGridItemVm transformDelegate_Items(AdvancedObservableCollection<PropertyGridItemVm> collection, object entry)
		{
			var result = new PropertyGridItemVm();
			result.Utility = Utility;
			result.DataProp.Value = (PropertyGridItemUtilityData)entry;
			result.CustomExpandCommand.AddSink(CustomExpandAggregateCommand);
			result.GroupName = result.DataProp.Value?.GroupName;
			return result;
		}
	}
}
