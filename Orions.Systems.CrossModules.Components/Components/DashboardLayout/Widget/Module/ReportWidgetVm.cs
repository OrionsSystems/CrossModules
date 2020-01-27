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

			ReportChartData = LoadReportChartData(reportResult, widget.CategoryFilter?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(it => it.Trim()).ToArray());

			IsLoadedReportResult = true;

			OnReportResultChanged?.Invoke();
			RaiseNotify(nameof(ReportChartData)); // Refresh UI.
			
		}

		public static ReportChartData LoadReportChartData(IReportResult reportResult, string[] categoryFilters)
		{
			var result = new ReportChartData();

			if (reportResult == null)
				return result;

			try
			{
				List<HyperMetadataReportResult> sources = new List<HyperMetadataReportResult>();
				if (reportResult is HyperMetadataReportResult metadataReport)
				{
					sources.Add(metadataReport);
				}
				else if (reportResult is MultiReportResult multiReport)
				{
					if (multiReport.Data is MultiReportData multiReportData)
						sources.AddRange(multiReportData.GetHyperReportResults());

				}

				foreach (var report in sources ?? Enumerable.Empty<HyperMetadataReportResult>())
				{
					var categories = report.ReportData.ColumnsDefinitions.Select(it => it.Title).ToList();
					var rowsDef = report.ReportData.RowsDefinitions.ToList();
					var rowData = report.ReportData.RowsCells;

					var resultCategoryRange = new List<string>();


					for (var i = 0; i < categories.Count; i++)
					{
						var categoryTitle = categories[i];

						if (categoryFilters?.Any() == true && !categoryFilters.Contains(categoryTitle))
						{
							continue;
						}

						var chartSeries = new ReportSeriesChartData();
						var existingSeries = result.Series.FirstOrDefault(it => it.Name == categoryTitle);
						
						if (existingSeries != null)
						{
							
							chartSeries = existingSeries;
						}
						else
						{
							chartSeries.Name = categoryTitle;
							resultCategoryRange.Add(categoryTitle);
						}

						for (var rowIndex = 0; rowIndex < rowData.Length; rowIndex++)
						{
							var rowEl = rowData[rowIndex];

							var reportRowEl = rowsDef[rowIndex];
							var label = reportRowEl.Title;

							var data = rowEl[i].Values.FirstOrDefault();

							ReportSeriesChartDataItem chartItem = null;
							var existingChartItem = chartSeries.Data.FirstOrDefault(it => it.Label == label);
							if (chartItem == null)
							{								
								chartItem = new ReportSeriesChartDataItem
								{
									Value = Convert.ToUInt16(data.ToString()),
									Label = label
								};

								if (result.IsDateAxis)
								{
									try
									{

										var position = ReportData.ParseTimePosition(label);
										if (!position.HasValue)
											result.IsDateAxis = false;
										else
										{
											chartItem.DatePosition = position.Value;
											chartItem.StreamPosition = ReportData.ParseStreamPosition(label);
										}
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
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Assert(false, ex.Message);
			}
		
			return result;
		}

	}
}
