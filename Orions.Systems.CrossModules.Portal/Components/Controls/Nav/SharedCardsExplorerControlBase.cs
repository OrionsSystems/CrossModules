using Orions.Common;
using Orions.SDK;
using Orions.Systems.CrossModules.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Util;

namespace Orions.Systems.CrossModules.Portal.Components
{
	[ViewModel(typeof(LevelModuleVm), true)]
	public class SharedCardsExplorerControlBase : BaseBlazorComponent, ICardControl
	{
		[Parameter]
		public IModuleVm SelectedItem { get; set; }

		protected LevelModuleVm Context
		{
			get
			{
				if (SelectedItem == null) return null;
				return SelectedItem as LevelModuleVm;
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

			if (SelectedItem == null) return;

			var levelModule = SelectedItem as LevelModuleVm;

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

		private void IsLoaded()
		{
			foreach (var value in Enum.GetValues(typeof(LevelModuleVm.TimeRanges)))
			{
				//this.radComboBoxTimeRange.Items.Add(value);
			}

			//this.radComboBoxTimeRange.SelectedItem = LevelModuleVm.TimeRanges.All;
		}

		private void ChangeCollapseAllButtonVisibility()
		{
			//TODO
		}

		//private void OnDataContextChanged(LevelModuleVm oldLvm, LevelModuleVm lvm)
		//{
		//	var oldLvm = e.OldValue as LevelModuleVm;
		//	if (oldLvm != null)
		//	{
		//		//if (oldLvm.SolutionVmProp.Value != null)
		//		//	oldLvm.SolutionVmProp.Value.RefreshCommand.Executed -= RefreshCommand_Executed;

		//		oldLvm.LevelIndexProp.ValueChanged -= LevelIndexProp_ValueChanged;

		//		oldLvm.NotifyLevelGenerated -= Lvm_NotifyLevelGenerated;
		//	}

		//	var lvm = e.NewValue as LevelModuleVm;
		//	if (lvm != null)
		//	{
		//		//if (lvm.SolutionVmProp.Value != null)
		//		//	lvm.SolutionVmProp.Value.RefreshCommand.Executed += RefreshCommand_Executed;

		//		lvm.LevelIndexProp.ValueChanged += LevelIndexProp_ValueChanged;

		//		lvm.NotifyLevelGenerated += Lvm_NotifyLevelGenerated;
		//	}

		//	LevelIndexProp_ValueChanged(null, 0);
		//}

		//private void LevelIndexProp_ValueChanged(INotifyProperty<int> prop, int newValue)
		//{
		//	var d = this.DataContext as LevelModuleVm;
		//	var entity = d.EntityAs<ModuleEntity>();

		//	collapseAllButton.IsChecked = entity.GetLevelViewCollapsed(d.CurrentLevelProp.Value?.OwnerVm?.ToString() ?? "");

		//	// Reset search when switching levels.
		//	this.textBoxSearch.Text = string.Empty;
		//}

		//private void textBoxSearch_GotFocus(object sender, RoutedEventArgs e)
		//{
		//	textBlockSearch.Visibility = Visibility.Collapsed;
		//}

		//private void TextBoxSearch_LostFocus(object sender, RoutedEventArgs e)
		//{
		//	if (this.textBoxSearch.Text.Trim().Length == 0)
		//		textBlockSearch.Visibility = Visibility.Visible;
		//}

		protected void textBoxSearch_KeyDown(KeyboardEventArgs e)
		{
			if (e.Key == Key.Enter.ToString())
			{
				//Force an update and try to release focus.
					
					//TODO !?
					//textBoxSearch.GetBindingExpression(TextBox.TextProperty).UpdateSource();
					//textBlockSearch.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
				}

			//{
			//	var itemsVm = textBoxSearch.DataContext as IItemsModuleVm;
			//	itemsVm.SearchProp.Value = textBoxSearch.Text;
			//}
			}

			//private void UserControl_Drop(object sender, DragEventArgs e)
			//{
			//	var module = this.DataContext as IDropModule;
			//	module?.OnDrop(e.Data);
			//}

			//private void UpdateCollapseButton()
			//{
			//	var d = this.DataContext as LevelModuleVm;
			//	var entity = d.EntityAs<ModuleEntity>();
			//	entity.SetLevelViewCollapsed(d.CurrentLevelProp.Value?.OwnerVm?.ToString() ?? "", collapseAllButton.IsChecked == true);

			//	var entryVms = sharedCardsControl.ItemsSource.OfType<NavigationEntryVm>().ToList();

			//	foreach (var entry in entryVms)
			//	{
			//		if (entry.GroupName != null)
			//		{
			//			entry.IsVisible.Value = !collapseAllButton.IsChecked.Value;
			//		}
			//	}
			//}

			//private void CollapseAllButton_Checked(object sender, RoutedEventArgs e)
			//{
			//	UpdateCollapseButton();
			//}

			//private void CollapseAllButton_Unchecked(object sender, RoutedEventArgs e)
			//{
			//	UpdateCollapseButton();
			//}

			//private void radComboBoxTimeRange_SelectionChanged(object sender, SelectionChangedEventArgs e)
			//{
			//	if (this.ViewModel<LevelModuleVm>() != null && this.radComboBoxTimeRange.SelectedItem != null)
			//	{
			//		var value = (LevelModuleVm.TimeRanges)this.radComboBoxTimeRange.SelectedItem;
			//		this.ViewModel<LevelModuleVm>().TimeRangeProp.Value = value;

			//		var t = this.ViewModel<LevelModuleVm>().RefreshCurrentLevelAsync();
			//	}
			//}
		}
}
