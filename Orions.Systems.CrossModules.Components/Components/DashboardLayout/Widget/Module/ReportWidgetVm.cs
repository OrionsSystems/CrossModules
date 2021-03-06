﻿using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Reporting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	/// <summary>
	/// Base class for Report specific widgets.
	/// </summary>
	public class ReportWidgetVm<WidgetType> : WidgetVm<WidgetType>
		where WidgetType : IReportDashboardWidget
	{
		public Report Report { get; private set; }

		public ReportChartData ReportChartData { get; private set; }

		public bool IsLoadedReportResult { get; private set; }

		public string ReportName { get { return Report?.Name; } }

		public bool ReportHasName { get { return !string.IsNullOrWhiteSpace(ReportName); } }

		public delegate void ReportResultChangedHandler();

		public event ReportResultChangedHandler OnReportResultChanged;

		public ReportWidgetVm()
		{
		}

		public override async Task HandleFiltersChangedAsync()
		{
			if (this.Widget is ActiveFilterReportChartWidget active && active.AllowFiltrationTarget)
				await RefreshReportResultData();
		}

		public async Task RefreshReportResultData()
		{
			var widget = this.Widget as ReportBaseWidget;
			var dataSource = widget?.DataSource;

			ReportChartData = null;

			OnReportResultChanged?.Invoke();
			RaiseNotify(nameof(ReportChartData)); // Refresh UI.

			if (dataSource == null)
				return;

			IsLoadedReportResult = false;

			//OnReportResultChanged?.Invoke();
			//RaiseNotify(nameof(ReportChartData)); // Refresh UI - loader.

			try
			{

				var context = new WidgetDataSourceContext();
				context.HyperStore = this.HyperStore;

				// Only if we are allowed as a active filtering target, do we set the filters.
				if (widget is ActiveFilterReportChartWidget activeReportWidget && activeReportWidget.AllowFiltrationTarget)
					context.GroupFilterData = this.DashboardVm?.ObtainFilterGroup(widget.FilterGroup);

				var reportResult = await dataSource.GenerateFilteredReportResultAsync(context);
				if (reportResult == null)
				{
					Logger.Instance.Error("Failed to load report result");
					return;
				}

				Report = reportResult.FirstOrDefault();

				if (Report == null || Report.ColumnsDefinitions.Length == 0)
				{
					return;
				}

				var includeCategoryFilter = widget.DataSource.IncludeCategories?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(it => it.Trim()).ToArray();
				var excludeCategoryFilter = widget.DataSource.ExcludeCategories?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(it => it.Trim()).ToArray();
				
				ReportChartData = LoadReportChartData(Report, includeCategoryFilter, excludeCategoryFilter, widget.DataSource.Uppercase);

				if (ReportChartData != null)
				{
					ReportChartData.MapIcons(dataSource.IconMapping);
					await LoadAllIcons(ReportChartData);
				}
			}
			finally
			{
				IsLoadedReportResult = true;

				OnReportResultChanged?.Invoke();
				RaiseNotify(nameof(ReportChartData)); // Refresh UI.
			}
		}

		private async Task LoadAllIcons(ReportChartData data) 
		{
			var docsIds = data.Series?.Where(it=>it.IconDocument != null)
				.Select(it => it.IconDocument.Value).ToArray();

			if (docsIds.Length == 0)
				return;

			var args = new RetrieveHyperDocumentsArgs();
			args.DocumentsIds = docsIds;
			
			var results = await this.HyperStore.ExecuteAsync(args);

			if (args.ExecutionResult.IsNotSuccess)
				return;

			foreach (var item in results) {
				var icon = item?.GetPayload<UniIconResource>();
				if (icon != null) {
					var chartData = data.Series.FirstOrDefault(it => it.IconDocument.GetValueOrDefault().Equals(item.Id));
					chartData.Icon = icon;
				}
			}
		}

		protected virtual string FormatData(object data)
		{
			string appendinx = "";

			if (this.Widget?.AppendPercentageSign == true)
				appendinx += "%";

			return Convert.ToString(data) + appendinx;
		}

		ReportChartData LoadReportChartData(Report report, 
			string[] includeCategoryFilters, 
			string[] excludeCategoryFilters,
			bool toUpper = false)
		{
			var result = new ReportChartData();

			if (report == null)
				return result;

			try
			{
				if (report.ColumnsDefinitions == null)
					return result;

				var columns = report.ColumnsDefinitions;
				var rowData = report.Rows;

				var resultCategoryRange = new List<string>();

				for (var columnIndex = 0; columnIndex < columns.Length; columnIndex++)
				{
					var columnTemplate = columns[columnIndex];
					var columnTitle = columnTemplate.Title;

					if (includeCategoryFilters?.Any() == true && !includeCategoryFilters.Contains(columnTitle))
						continue;

					if (excludeCategoryFilters?.Any() == true && excludeCategoryFilters.Contains(columnTitle))
						continue;

					if (toUpper) {
						columnTitle = columnTitle.ToUpper();
					}

					var chartSeries = new ReportSeriesChartData();
					var existingSeries = result.Series.FirstOrDefault(it => it.Name == columnTitle);

					if (existingSeries != null)
					{

						chartSeries = existingSeries;
					}
					else
					{
						chartSeries.Name = columnTitle;
						resultCategoryRange.Add(columnTitle);
					}

					for (var rowIndex = 0; rowIndex < rowData.Length; rowIndex++)
					{
						var row = rowData[rowIndex];

						var label = report.RowsDefinitions[rowIndex].Title;
						if(string.IsNullOrWhiteSpace(label)) 
							label = row.Template.Title;

						var cell = row.Cells[columnIndex];
						var data = cell.Values.FirstOrDefault();

						ReportSeriesChartDataItem chartItem = null;
						var existingChartItem = chartSeries.Data.FirstOrDefault(it => it.Label == label);
						if (chartItem == null)
						{
							chartItem = new ReportSeriesChartDataItem
							{
								CategoryName = columnTitle,
								Value = FormatData(data),
								Label = label,

								RowTemplate = row.Template,
								ColumnTemplate = columnTemplate,

								Cell = cell,
							};

							if (result.IsDateAxis)
							{
								try
								{
									result.IsDateAxis = row.Template?.StartDateTime.HasValue == true;

									//var position = Report.ParseTimePosition(label);
									//if (!position.HasValue)
									//{
									//	result.IsDateAxis = false;
									//}
									//else
									//{
									//	chartItem.DatePosition = position.Value;
									//	chartItem.StreamPosition = Report.ParseStreamPosition(label);
									//}
								}
								catch (Exception ex)
								{
									result.IsDateAxis = false;
								}
							}

							chartSeries.Data.Add(chartItem);
						}
						else
						{
							chartItem.Value += Convert.ToUInt16(data.ToString());
						}
					}

					result.AddCategoryRange(resultCategoryRange.ToArray());
					result.Series.Add(chartSeries);
				}
			}
			catch (Exception ex)
			{
				Logger.Instance.Error(typeof(ReportWidgetVm<>), nameof(LoadReportChartData), ex);
				System.Diagnostics.Debug.Assert(false, ex.Message);
			}

			return result;
		}

	}
}
