using Microsoft.AspNetCore.Components.Web;

using Orions.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardVm : BlazorVm
	{
		public DashboardData Source { get; set; } = new DashboardData();

		public List<IDashboardWidget> AvailableWidgets { get; private set; } = new List<IDashboardWidget>();

		public DashboardVm()
		{
			LoadAvailableWidget();
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

		public PropertyGrid PropGrid { get; set; }

		public bool IsShowModalWidget { get; set; }
		public bool IsShowProperty { get; private set; }

		private DashboardColumn SelectedColumn { get; set; }

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

		public void AddSelectedWidget(MouseEventArgs e, IDashboardWidget widget)
		{
			if (widget != null && SelectedColumn != null) SelectedColumn.Widget = widget;

			IsShowModalWidget = false;
		}

		public void OnCancelProperty()
		{
			PropGrid.CleanSourceCache();
			IsShowProperty = false;
		}

		public async Task<object> GetSelectedColumnWidget()
		{
			//PropGrid.CleanSourceCache();
			return SelectedColumn.Widget;
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

			AvailableWidgets = new List<IDashboardWidget>();
			foreach (var t in availableTypes.Where(it => it.IsAbstract == false))
			{
				var widget = Activator.CreateInstance(t);
				AvailableWidgets.Add(widget as IDashboardWidget);
			}
		}
	}
}
