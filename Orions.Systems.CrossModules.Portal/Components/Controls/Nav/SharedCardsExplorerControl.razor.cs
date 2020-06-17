using Orions.Common;
using Orions.SDK;
using Orions.Systems.CrossModules.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Util;

namespace Orions.Systems.CrossModules.Portal.Components
{
	[ViewModel(typeof(LevelModuleVm), true)]
	public partial class SharedCardsExplorerControl : BaseBlazorComponent
	{
		protected LevelModuleVm Context
		{
			get
			{
				if (DataContext == null) return null;
				return DataContext as LevelModuleVm;
			}
		}

		public class TimeRangeItem
		{
			public string Name { get; set; }
			public int Value { get; set; }
			public LevelModuleVm.TimeRanges Source { get; set; }

			public TimeRangeItem(LevelModuleVm.TimeRanges enumValue)
			{
				Value = Convert.ToInt32(enumValue);
				Name = enumValue.ToString();
				Source = enumValue;
			}
		}

		protected int? SelectedComboBoxTimeRangeIndex { get; set; }
		//protected TimeRangeItem SelectedComboBoxTimeRange { get; set; }

		protected List<TimeRangeItem> ComboBoxTimeRangeItems { get; private set; } = new List<TimeRangeItem>();

		protected bool IsToggleChecked { get; set; } = true;

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			foreach (var value in Enum.GetValues(typeof(LevelModuleVm.TimeRanges)))
			{
				var enumValue = (LevelModuleVm.TimeRanges)value;
				var item = new TimeRangeItem(enumValue);
				ComboBoxTimeRangeItems.Add(item);
			}

			SelectedComboBoxTimeRangeIndex = Convert.ToInt32(LevelModuleVm.TimeRanges.All);
		}

		protected void OnToggleButton()
		{
			IsToggleChecked = !IsToggleChecked;
		}

		protected async Task RefreshLevelCommand()
		{
			if (Context == null) return;

			await Context.RefreshCurrentLevelAsync();
		}

		protected void ComboBoxTimeRange_SelectionChanged(Syncfusion.EJ2.Blazor.DropDowns.ChangeEventArgs<string> args)
		{
			//var comboVal = args.Value;

			if (DataContext == null) return;

			var levelModule = DataContext as LevelModuleVm;

			if (levelModule != null)
			{
				var value = LevelModuleVm.TimeRanges.All;

				if (SelectedComboBoxTimeRangeIndex != null)
				{
					value = (LevelModuleVm.TimeRanges)SelectedComboBoxTimeRangeIndex;
				}

				levelModule.TimeRangeProp.Value = value;
				var t = levelModule.RefreshCurrentLevelAsync();

				StateHasChanged();
			}
		}

		protected override void OnDataContextAssigned(object dataContext)
		{
			base.OnDataContextAssigned(dataContext);

			// TODO
		}

		protected override void OnStateHasChanged()
		{
			base.OnStateHasChanged();
		}

		private void ChangeCollapseAllButtonVisibility()
		{
			//TODO
		}

		private void OnDataContextChanged(object newDataContext)
		{
			var oldLvm = DataContext as LevelModuleVm;
			if (oldLvm != null)
			{
				//if (oldLvm.SolutionVmProp.Value != null)
				//	oldLvm.SolutionVmProp.Value.RefreshCommand.Executed -= RefreshCommand_Executed;

				oldLvm.LevelIndexProp.ValueChanged -= LevelIndexProp_ValueChanged;

				oldLvm.NotifyLevelGenerated -= Lvm_NotifyLevelGenerated;
			}

			var lvm = newDataContext as LevelModuleVm;
			if (lvm != null)
			{
				//if (lvm.SolutionVmProp.Value != null)
				//	lvm.SolutionVmProp.Value.RefreshCommand.Executed += RefreshCommand_Executed;

				lvm.LevelIndexProp.ValueChanged += LevelIndexProp_ValueChanged;

				lvm.NotifyLevelGenerated += Lvm_NotifyLevelGenerated;
			}

			LevelIndexProp_ValueChanged(null, 0);
		}

		private void Lvm_NotifyLevelGenerated()
		{
			ChangeCollapseAllButtonVisibility();
		}

		private void LevelIndexProp_ValueChanged(INotifyProperty<int> prop, int newValue)
		{
			var d = DataContext as LevelModuleVm;
			var entity = d.EntityAs<ModuleEntity>();

			var isChecked = entity.GetLevelViewCollapsed(d.CurrentLevelProp.Value?.OwnerVm?.ToString() ?? "");
			//collapseAllButton.IsChecked = isChecked;

			// Reset search when switching levels.
			//this.textBoxSearch.Text = string.Empty;
		}

		protected void textBoxSearch_KeyDown(KeyboardEventArgs e)
		{
			if (e.Key == Key.Enter.ToString())
			{
				
			}
		}
	}
}
