using System;
using System.Collections.Generic;
using System.Text;
using Orions.Common;
using System.Linq;
using System.Threading.Tasks;
using Orions.Infrastructure.Reporting;
using Microsoft.AspNetCore.Components;

namespace Orions.Systems.CrossModules.Components
{
	/// <summary>
	/// Base class for Report specific widgets.
	/// </summary>
	public class ReportWidgetVm<WidgetType> : WidgetVm<WidgetType>
		where WidgetType : IDashboardWidget
	{
		public IReportResult Report { get; private set; }

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
			await RefreshReportResultData();
		}

		public async Task RefreshReportResultData()
		{
			var widget = this.Widget as ReportBaseWidget;
			var dataSource = widget?.DataSource;
			if (dataSource == null)
				return;

			var context = new WidgetDataSourceContext();
			context.HyperStore = this.HyperStore;

			context.DynamicFilter = this.DashboardVm?.GetFilterGroup(widget.FilterGroup);

			var reportResult = await dataSource.GenerateFilteredReportResultAsync(context);
			if (reportResult == null)
			{
				Logger.Instance.Error("Cannot load report result");
				IsLoadedReportResult = true;
				return;
			}

			Report = reportResult;

			ReportChartData = LoadReportChartData(reportResult, widget.CategoryFilter?.Split(',').Select(it => it.Trim()).ToArray());

			IsLoadedReportResult = true;

			RaiseNotify(nameof(ReportChartData)); // Refresh UI.
			OnReportResultChanged?.Invoke();
		}

		public static ReportChartData LoadReportChartData(IReportResult report, string[] categoryFilters)
		{
			var result = new ReportChartData();

			if (report == null) 
				return result;

			try
			{
				var categories = report.Data.ColumnsDefinitions.Select(it => it.Title).ToList();
				var rowsDef = report.Data.RowsDefinitions.ToList();

				result.Categories.AddRange(categories);

				var rowData = report.Data.RowsCells;

				for (var i = 0; i < categories.Count; i++)
				{
					var categoryTitle = categories[i];

					if (categoryFilters?.Any() == true && !categoryFilters.Contains(categoryTitle))
					{
						continue;
					}

					var chartSeries = new ReportSeriesChartData();
					chartSeries.Name = categoryTitle;

					for (var rowIndex = 0; rowIndex < rowData.Length; rowIndex++)
					{
						var rowEl = rowData[rowIndex];

						var reportRowEl = rowsDef[rowIndex];
						var timeEl = reportRowEl.Title;

						var data = rowEl[i].Values.FirstOrDefault();

						var chartItem = new ReportSeriesChartDataItem
						{
							Count = Convert.ToUInt16(data.ToString()),
							Time = timeEl
						};

						if (result.IsDateAxis)
						{
							try
							{
								chartItem.DatePosition = ReportData.TryParseTimePosition(timeEl) ?? throw new OrionsException("Failed to parse");
								chartItem.StreamPosition = ReportData.ParseStreamPosition(timeEl);
							}
							catch (Exception)
							{
								result.IsDateAxis = false;
							}
						}

						chartSeries.Data.Add(chartItem);
					}

					result.Series.Add(chartSeries);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Assert(false, ex.Message);
			}

			return result;
		}

	}
}
