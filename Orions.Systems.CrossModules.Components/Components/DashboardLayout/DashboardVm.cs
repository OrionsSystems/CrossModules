using Microsoft.AspNetCore.Components.Web;

using Orions.Common;
using Orions.Node.Common;
using Orions.Infrastructure.HyperMedia;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardVm : BlazorVm
	{
		object _syncRoot = new object();

		public DashboardData Source { get; set; } = new DashboardData();

		public List<Type> AvailableWidgets { get; private set; } = new List<Type>();

		public UniFilterData DynamicFilter { get; set; } = new MultiFilterData();

		public PropertyGrid PropGrid { get; set; }

		public bool IsShowModalWidget { get; set; }
		public bool IsShowProperty { get; private set; }

		private DashboardColumn SelectedColumn { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		List<WidgetVm> _widgets = new List<WidgetVm>();

		public WidgetVm[] WidgetsVms
		{
			get
			{
				lock (_syncRoot)
					return _widgets.ToArray();
			}
		}

		public DashboardVm()
		{
			LoadAvailableWidget();
		}

		public void SetStringFilters(string[] filters)
		{
			if (DynamicFilter is MultiFilterData multiFilter)
			{
				multiFilter.Elements = new IUniFilterData[] { new TextFilterData() { 
					LabelsArray = filters, Mode = AndOr.Or, StringCompareMode = TextFilterData.StringComparisonMode.Contains } };
			}
			else
			{
				System.Diagnostics.Debug.Assert(false, "Main filter not set as expected");
			}
		}

		/// <summary>
		/// Run this to update dynamic filter widgets, so they can pick up changed filter values.
		/// </summary>
		public async Task UpdateDynamicWidgetsAsync()
		{
			foreach (var widgetVm in this.WidgetsVms)
			{
				await widgetVm.HandleFiltersChangedAsync();
			}
		}

		public void TryAddWidgetVm(WidgetVm vm)
		{
			lock (_syncRoot)
			{
				if (_widgets.Contains(vm) == false)
					_widgets.Add(vm);
			}
		}

		public void OnAddRow()
		{
			var row = new DashboardRow();
			row.Columns.AddLast(new DashboardColumn { Size = 12 });
			Source.Rows.AddLast(row);
		}

		public void SplitColumn(MouseEventArgs e, DashboardRow row, DashboardColumn column)
		{
			var size = column.Size % 2;
			var sizeF = column.Size / 2;

			column.Size = sizeF;
			column.ShowCommands = false;
			var newColumn = new DashboardColumn { Size = sizeF + size };

			var n = row.Columns.Find(column);
			row.Columns.AddAfter(n, newColumn);
		}

		public void DeleteColumn(MouseEventArgs e, DashboardRow row, DashboardColumn column)
		{
			var columnSize = column.Size;

			if (row.Columns.Count == 1)
			{
				Source.Rows.Remove(row);
				return;
			}

			var n = row.Columns.Find(column);
			var prevColumn = n.Previous;

			row.Columns.Remove(column);

			if (prevColumn == null)
			{
				var firstCol = row.Columns.FirstOrDefault();
				firstCol.Size += columnSize;
				return;
			}

			prevColumn.Value.Size += columnSize;
		}

		public void OnSwapColumns(MouseEventArgs e, DashboardRow row, DashboardColumn column)
		{

			var n = row.Columns.Find(column);
			var nextColumn = n.Next;

			if (nextColumn == null) return;

			row.Columns.Remove(nextColumn);
			row.Columns.AddBefore(n, nextColumn);

			column.ShowBetweenCommands = false;
		}

		public void OnSwapRows(MouseEventArgs e, DashboardRow row)
		{
			var r = Source.Rows.Find(row);
			var prevRow = r.Previous;

			if (prevRow == null) return;

			Source.Rows.Remove(prevRow);
			Source.Rows.AddAfter(r, prevRow);
		}

		public void IncreaseSizeLeft(MouseEventArgs e, DashboardRow row, DashboardColumn column)
		{
			var n = row.Columns.Find(column);
			var prevColumn = n.Previous;

			if (prevColumn == null || prevColumn.Value.Size == 1) return;

			prevColumn.Value.Size--;
			column.Size++;

			column.ShowBetweenCommands = false;
		}

		public void IncreaseSizeRight(MouseEventArgs e, DashboardRow row, DashboardColumn column)
		{
			var n = row.Columns.Find(column);
			var nextColumn = n.Next;

			if (nextColumn == null || nextColumn.Value.Size == 1) return;

			nextColumn.Value.Size--;
			column.Size++;

			column.ShowBetweenCommands = false;
		}

		public bool HasPreviousColumn(DashboardRow row, DashboardColumn column)
		{
			var n = row.Columns.Find(column);
			var prevColumn = n.Previous;

			if (prevColumn == null || prevColumn.Value.Size == 1) return false;

			return true;
		}

		public bool HasNextColumn(DashboardRow row, DashboardColumn column)
		{
			var n = row.Columns.Find(column);
			var nextColumn = n.Next;

			if (nextColumn == null || nextColumn.Value.Size == 1) return false;

			return true;
		}

		public void ShowCommands(MouseEventArgs e, DashboardColumn column)
		{
			column.ShowCommands = true;
		}

		public void HideCommands(MouseEventArgs e, DashboardColumn column)
		{
			column.ShowCommands = false;
		}

		public void CleanWidget(MouseEventArgs e, DashboardColumn column)
		{
			column.Widget = null;
		}

		public void OpenWidgetModal(MouseEventArgs e, DashboardColumn column)
		{
			if (AvailableWidgets != null && AvailableWidgets.Any())
			{
				IsShowModalWidget = true;
				SelectedColumn = column;
			}
		}

		public void OpenWidgetProperty(MouseEventArgs e, DashboardColumn column)
		{
			IsShowProperty = true;
			SelectedColumn = column;
		}

		public void AddSelectedWidget(MouseEventArgs e, Type widgetType)
		{
			if (widgetType != null && SelectedColumn != null) 
				SelectedColumn.Widget = LoadWidget(widgetType);

			IsShowModalWidget = false;
		}

		public void OnCancelProperty()
		{
			PropGrid.CleanSourceCache();
			IsShowProperty = false;
		}

		public Task<object> GetSelectedColumnWidget()
		{
			return Task.FromResult((object)SelectedColumn.Widget);
		}

		private bool _isStartDraging;
		private double _startClientX;

		public void OnMouseDownDraging(MouseEventArgs e)
		{
			_isStartDraging = true;
			_startClientX = e.ClientX;
		}

		public void OnMouseMoveDraging(
			MouseEventArgs e,
			DashboardRow row,
			DashboardColumn column)
		{
			var x = e.ClientX;
			var dif = x - _startClientX;

			if (_isStartDraging && (dif > 3 || dif < -3))
			{
				var n = row.Columns.Find(column);
				var prevColumn = n.Next;

				if (prevColumn == null) return;

				if (dif < -3)
				{
					if (column.Size == 1 || prevColumn.Value.Size == 12) return;

					prevColumn.Value.Size++;
					column.Size--;
				}

				if (dif > 3)
				{
					if (prevColumn.Value.Size == 1 || column.Size == 12) return;

					prevColumn.Value.Size--;
					column.Size++;
				}

				_isStartDraging = false;
			}
		}

		private void LoadAvailableWidget()
		{
			var availableTypes = ReflectionHelper.Instance.GatherTypeChildrenTypesFromAssemblies(typeof(IDashboardWidget));

			foreach (var t in availableTypes.Where(it => it.IsAbstract == false))
			{
				var widget = Activator.CreateInstance(t);
				AvailableWidgets.Add(t);
			}
		}

		private DashboardWidget LoadWidget(Type t)
		{
			var widget = Activator.CreateInstance(t);
			return widget as DashboardWidget;
		}
	}
}
