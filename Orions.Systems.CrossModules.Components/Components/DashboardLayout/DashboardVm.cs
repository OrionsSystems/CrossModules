using Microsoft.AspNetCore.Components.Web;

using Orions.Common;
using Orions.Node.Common;
using Orions.Infrastructure.HyperMedia;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Infrastructure.Reporting;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardVm : BlazorVm
	{
		static object g_syncRoot = new object();

		object _syncRoot = new object();

		static Type[] _widgetVmTypes = null;

		DashboardData _source = new DashboardData();
		public DashboardData Source
		{
			get
			{
				return _source;
			}

			set
			{
				_source = value;
				OnSourceSet();
			}
		}

		public List<Type> AvailableWidgets { get; private set; } = new List<Type>();

		Dictionary<string, MultiFilterData> _dynamicFiltersByGroup { get; set; } = new Dictionary<string, MultiFilterData>();

		/// <summary>
		/// The Propertyy grid has its own Vm, but we assign it one from here, to make sure it uses this one, so we can control it.
		/// </summary>
		public PropertyGridVm PropGridVm { get; set; } = new PropertyGridVm();

		public bool IsShowModalWidget { get; set; }
		public bool IsShowProperty { get; private set; }

		private DashboardColumn SelectedColumn { get; set; }

		IHyperArgsSink _hyperStore = null;
		public IHyperArgsSink HyperStore
		{
			get
			{
				return _hyperStore;
			}

			set
			{
				_hyperStore = value;
			}
		}

		Dictionary<IDashboardWidget, WidgetVm> _widgetsVms = new Dictionary<IDashboardWidget, WidgetVm>();

		public DashboardVm()
		{
			lock (g_syncRoot)
			{
				if (_widgetVmTypes == null)
				{// Cache this as it scans very many types.
					_widgetVmTypes = ReflectionHelper.Instance.GatherTypeChildrenTypesFromAssemblies(typeof(WidgetVm)).Where(it => it.IsAbstract == false).ToArray();
				}
			}

			LoadAvailableWidget();
		}

		protected virtual void OnSourceSet()
		{
			if (this.HyperStore == null)
			{
				System.Diagnostics.Debug.Assert(false, "HyperStore not assigned");
			}

			// Create the Vms for all the widgets of this dashboard.
			foreach (var row in this.Source.Rows)
			{
				foreach (var column in row.Columns)
				{
					if (column.Widget == null)
						continue;

					ObtainWidgetVm(column.Widget);
				}
			}
		}

		public WidgetVm ObtainWidgetVm(IDashboardWidget widget)
		{
			WidgetVm widgetVm;
			if (_widgetsVms.TryGetValue(widget, out widgetVm))
				return widgetVm;

			// Deduce vm type from the Widget type.
			var vmType = _widgetVmTypes.First(it => it.GetCustomAttributes(true).OfType<ConfigAttribute>().Any(it => it.ConfigType == widget.GetType()));

			var vm = (WidgetVm)Activator.CreateInstance(vmType);
			vm.HyperStore = this.HyperStore;
			vm.Widget = widget; // ** Order matters here!!
			vm.ParentVm = this; // Allows the Vms to take action prior to any UI being renderered.
			
			_widgetsVms[widget] = vm;

			return vm;
		}

		public async Task<Response> SaveChangesAsync()
		{
			var doc = new HyperDocument(this.Source);

			var args = new StoreHyperDocumentArgs(doc);
			await HyperStore.ExecuteAsync(args);

			return args.ExecutionResult.AsResponse();
		}

		MultiFilterData ObtainFilterData(string group)
		{
			if (group == null)
				group = "";

			MultiFilterData data;
			lock (_syncRoot)
			{
				if (_dynamicFiltersByGroup.TryGetValue(group, out data) == false)
				{
					data = new MultiFilterData();
					_dynamicFiltersByGroup[group] = data;
				}
			}

			return data;
		}

		public MultiFilterData GetFilterGroup(string group)
		{
			if (group == null)
				group = "";

			MultiFilterData result = null;
			lock (_syncRoot)
			{
				_dynamicFiltersByGroup.TryGetValue(group, out result);
			}

			return result;
		}

		public void SetDateTimeFilters(string group, DateTime? startTime, DateTime? endTime, ReportInstruction.Targets filterTarget)
		{
			var data = ObtainFilterData(group);

			if (startTime.HasValue == false && endTime.HasValue == false)
			{
				data.Elements = data.Elements.Where(it => it is DateTimeFilterData == false).ToArray(); // Remove all time filters.
			}
			else
			{
				var dateTimeFilterData = data.ObtainElement<DateTimeFilterData>();
				dateTimeFilterData.StartTime = startTime;
				dateTimeFilterData.EndTime = endTime;
				dateTimeFilterData.Instruction = new ReportInstruction() { Target = filterTarget };
			}
		}

		public void ClearAllFilters()
		{
			lock (_syncRoot)
			{
				foreach (var g in _dynamicFiltersByGroup)
					g.Value.Clear();
			}

		}

		public void ClearFilters(string group)
		{
			var data = ObtainFilterData(group);

			data.Clear();
		}

		public void SetStringFilters(string group, string[] filters, ReportInstruction.Targets filterTarget)
		{
			var data = ObtainFilterData(group);

			if (filters == null || filters.Length == 0)
			{
				data.Elements = data.Elements.Where(it => it is TextFilterData == false).ToArray(); // Remove all string filters.
			}
			else
			{
				var textFilterData = data.ObtainElement<TextFilterData>();

				textFilterData.LabelsArray = filters;
				textFilterData.Mode = AndOr.Or;
				textFilterData.StringCompareMode = StringComparisonMode.Contains;
				textFilterData.Instruction = new ReportInstruction() { Target = filterTarget };
			}
		}

		/// <summary>
		/// Run this to update dynamic filter widgets, so they can pick up changed filter values.
		/// </summary>
		public async Task UpdateDynamicWidgetsAsync()
		{
			foreach (var widgetVm in _widgetsVms.Select(it => it.Value))
			{
				await widgetVm.HandleFiltersChangedAsync();
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
			PropGridVm.CleanSourceCache();
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
