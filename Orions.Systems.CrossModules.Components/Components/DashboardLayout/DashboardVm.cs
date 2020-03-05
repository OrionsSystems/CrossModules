using Microsoft.AspNetCore.Components.Web;

using Orions.Common;
using Orions.Node.Common;
using Orions.Infrastructure.HyperMedia;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

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

				//lock (_syncRoot)
				//{
				//	_widgetsVms.Clear();
				//}

				OnSourceSet();
			}
		}

		public List<Type> AvailableWidgets { get; private set; } = new List<Type>();

		Dictionary<string, DashboardFilterGroupData> _dynamicFiltersByGroup { get; set; } = new Dictionary<string, DashboardFilterGroupData>();

		public UniPair<string, DashboardFilterGroupData>[] FilterGroups
		{
			get
			{
				lock (_syncRoot)
				{
					return _dynamicFiltersByGroup.Select(it => new UniPair<string, DashboardFilterGroupData>(it.Key, it.Value)).ToArray();
				}
			}
		}


		/// <summary>
		/// The Propertyy grid has its own Vm, but we assign it one from here, to make sure it uses this one, so we can control it.
		/// </summary>
		public PropertyGridVm PropGridVm { get; set; } = new PropertyGridVm();

		public bool IsShowModalWidget { get; set; }
		public bool IsShowProperty { get; private set; }
		public bool IsShowModalImportProject { get; set; }

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

		public void ClearWidgets()
		{
			lock (_syncRoot)
			{
				_widgetsVms.Clear();
			}
		}

		/// <summary>
		/// When running as part of another dashboard, synchronize to its filtering system.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public async Task FromParent_HandleFiltersChangedAsync(UniPair<string, DashboardFilterGroupData>[] filterPairs)
		{
			// Reset to match the parent.
			lock (_syncRoot)
			{
				_dynamicFiltersByGroup.Clear();
				foreach (var pair in filterPairs)
				{
					_dynamicFiltersByGroup[pair.Key] = pair.Value;
				}
			}

			await this.UpdateDynamicWidgetsFilteringAsync();
		}

		protected virtual void OnSourceSet()
		{
			if (this.HyperStore == null)
			{
				System.Diagnostics.Debug.Assert(false, "HyperStore not assigned");
			}

			// Create the Vms for all the widgets of this dashboard.
			foreach (var row in this.Source?.Rows ?? new LinkedList<DashboardRow>())
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

		public DashboardFilterGroupData ObtainFilterGroup(IDashboardWidget widget)
		{
			return this.ObtainFilterGroup(widget.FilterGroup);
		}

		public DashboardFilterGroupData ObtainFilterGroup(string group)
		{
			if (group == null)
				group = "";

			DashboardFilterGroupData data;
			lock (_syncRoot)
			{
				if (_dynamicFiltersByGroup.TryGetValue(group, out data) == false)
				{
					data = new DashboardFilterGroupData();
					_dynamicFiltersByGroup[group] = data;
				}
			}

			return data;
		}

		//public DashboardGroupData GetFilterGroup(string group)
		//{
		//	if (group == null)
		//		group = "";

		//	DashboardGroupData result = null;
		//	lock (_syncRoot)
		//	{
		//		_dynamicFiltersByGroup.TryGetValue(group, out result);
		//	}

		//	return result;
		//}

		//public void SetDateTimeFilters(string group, DateTime? startTime, DateTime? endTime)
		//{
		//	DashboardGroupData data = ObtainFilterData(group);

		//	//if (startTime.HasValue == false && endTime.HasValue == false)
		//	//{
		//	//	data.Elements = data.Elements.Where(it => it is DateTimeFilterData == false).ToArray(); // Remove all time filters.
		//	//}
		//	//else
		//	//{
		//	//	var dateTimeFilterData = data.ObtainElement<DateTimeFilterData>();
		//	//	dateTimeFilterData.StartTime = startTime;
		//	//	dateTimeFilterData.EndTime = endTime;
		//	//	dateTimeFilterData.Instruction = new ReportFilterInstruction() { Target = filterTarget };
		//	//}
		//}

		public void ClearAllFilterGroups()
		{
			lock (_syncRoot)
			{
				foreach (var g in _dynamicFiltersByGroup)
					g.Value.Clear();
			}
		}

		//public void SetStringFilters(string group, string[] filters, ReportFilterInstruction.Targets filterTarget)
		//{
		//	var data = ObtainFilterData(group);

		//	//if (filters == null || filters.Length == 0)
		//	//{
		//	//	data.Elements = data.Elements?.Where(it => it is TextFilterData == false).ToArray(); // Remove all string filters.
		//	//}
		//	//else
		//	//{
		//	//	var textFilterData = data.ObtainElement<TextFilterData>();

		//	//	textFilterData.LabelsArray = filters;
		//	//	textFilterData.Mode = AndOr.Or;
		//	//	textFilterData.StringCompareMode = StringComparisonMode.Contains;
		//	//	textFilterData.Instruction = new ReportFilterInstruction() { Target = filterTarget };
		//	//}
		//}

		/// <summary>
		/// Run this to update dynamic filter widgets, so they can pick up changed filter values.
		/// </summary>
		public async Task UpdateDynamicWidgetsFilteringAsync()
		{
			var tasks = new List<Task>();
			foreach (var widgetVm in _widgetsVms.Select(it => it.Value))
			{
				if (widgetVm.Widget.IsVisible == false)
					continue;

				tasks.Add(widgetVm.HandleFiltersChangedAsync());
			}

			await Task.WhenAll(tasks);
		}

		public void ImportProject(byte[] bytes, bool isNew = true)
		{
			var json = Encoding.Default.GetString(bytes);

			if (string.IsNullOrWhiteSpace(json)) 
				return;

			var res = JsonHelper.Deserialize<DashboardData>(json);
			
			if(isNew) 
				res.Id = IdHelper.GenerateId();

			//update and save
			if (res != null) 
				Source = res;
		}

		public string LoadDashboardAsJson()
		{
			return JsonHelper.Serialize(Source);
		}

		public string SerializeWidget(IDashboardWidget widget) {
			return JsonHelper.Serialize(widget);
		}

		public void LoadSavedWidget(string json)
		{
			var widget =  JsonHelper.Deserialize<DashboardWidget>(json);
			if (widget == null) return;
			SelectedColumn.Widget = widget;
			IsShowModalWidget = false;
		}

		#region Dashboad Desing Operations

		public void OnAddRow()
		{
			var row = new DashboardRow();
			row.Columns.AddLast(new DashboardColumn { Size = 12 });
			Source.Rows.AddLast(row);
		}

		// split only first level
		public void InitSplitElementVerical(MouseEventArgs e, DashboardRow row, DashboardColumn column) 
		{
			if (column.InnerRows.Count != 0) return; 

			var row1 = new DashboardRow();
			row1.Columns.AddFirst(new DashboardColumn { Size = 12 });

			var row2 = new DashboardRow();
			row2.Columns.AddFirst(new DashboardColumn { Size = 12 });
			column.InnerRows.AddFirst(row1);
			column.InnerRows.AddFirst(row2);
		}

		public void AddInnerRow(MouseEventArgs e, DashboardRow row, DashboardColumn column)
		{
			var newRow = new DashboardRow();
			newRow.Columns.AddFirst(new DashboardColumn { Size = 12 });
			
			column.InnerRows.AddLast(newRow);
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

		public void DeleteColumn(MouseEventArgs e, DashboardRow row, DashboardColumn column, DashboardColumn parrernColumn = null)
		{
			var columnSize = column.Size;

			if (row.Columns.Count == 1)
			{
				if (parrernColumn != null)
				{
					parrernColumn.InnerRows.Remove(row);
				}
				else {
					Source.Rows.Remove(row);
				}
				
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

		public void OnSwapInnerRows(MouseEventArgs e, DashboardColumn parentColumn, DashboardRow row)
		{
			if (parentColumn == null || row == null) return;

			var r = parentColumn.InnerRows.Find(row);
			var prevRow = r.Previous;

			if (prevRow == null) return;

			parentColumn.InnerRows.Remove(prevRow);
			parentColumn.InnerRows.AddAfter(r, prevRow);
		}

		public void OnSwapRows(MouseEventArgs e, DashboardRow row)
		{
			var r = Source.Rows.Find(row);
			var prevRow = r.Previous;

			if (prevRow == null) return;

			Source.Rows.Remove(prevRow);
			Source.Rows.AddAfter(r, prevRow);
		}

		public void CloneRow(MouseEventArgs e, DashboardRow row)
		{
			var r = Source.Rows.Find(row);
			//TODO add copy implementation !!!
			
			//Source.Rows.AddAfter(r, r.Value);
		}

		public void CloneInnerRow(MouseEventArgs e, DashboardColumn parentColumn, DashboardRow row)
		{
			var r = Source.Rows.Find(row);
			//TODO add copy implementation !!!

			//Source.Rows.AddAfter(r, r.Value);
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

		public void ToggleVisibility(MouseEventArgs e, DashboardColumn column)
		{
			column.Widget.IsVisible = !column.Widget.IsVisible;
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

		public void OnMouseMoveDraging(MouseEventArgs e, DashboardRow row, DashboardColumn column)
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

		#endregion

		private void LoadAvailableWidget()
		{
			var availableTypes = ReflectionHelper.Instance.GatherTypeChildrenTypesFromAssemblies(typeof(IDashboardWidget));

			foreach (var t in availableTypes.Where(it => it.IsAbstract == false))
			{
				var widget = Activator.CreateInstance(t);
				AvailableWidgets.Add(t);
			}
		}

		public DashboardWidget LoadWidget(Type t)
		{
			var widget = Activator.CreateInstance(t);
			return widget as DashboardWidget;
		}

		public bool IsDashboadContainer(IDashboardWidget widget)
		{
			return widget is DashboardContainerWidget;
		}

		//public async Task<DashboardData> GetDashboardContainerData(IDashboardWidget widget)
		//{
		//	var vm = ObtainWidgetVm(widget);

		//	var containerWidget = vm.Widget as DashboardContainerWidget;

		//	var documentId = containerWidget.Dashboard;

		//	if (documentId != null)
		//	{
		//		var args = new RetrieveHyperDocumentArgs(documentId.Value);
		//		var doc = await this.HyperStore.ExecuteAsync(args);

		//		if (args.ExecutionResult.IsNotSuccess)
		//			return null;

		//		var dashboard = doc?.GetPayload<DashboardData>();

		//		return dashboard;
		//	}

		//	return null;
		//}
	}
}
