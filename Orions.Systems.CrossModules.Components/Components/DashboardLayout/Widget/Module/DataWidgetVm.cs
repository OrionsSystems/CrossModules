using Orions.Common;
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
	/// Base class for Data specific widgets.
	/// </summary>
	public class DataWidgetVm<WidgetType> : WidgetVm<WidgetType>
		where WidgetType : IReportDashboardWidget
	{
		public Report Report { get; private set; }

		public ReportData ReportData { get; private set; }

		public bool IsLoadedReportResult { get; private set; }

		public string ReportName { get { return Report?.Name; } }

		public bool ReportHasName { get { return !string.IsNullOrWhiteSpace(ReportName); } }

		public delegate void ReportResultChangedHandler();

		public event ReportResultChangedHandler OnReportResultChanged;

		public DataWidgetVm()
		{
		}

		public async Task RefreshReportResultData()
		{
			var widget = this.Widget as ReportBaseWidget;
			var dataSource = widget?.DataSource;

			ReportData = null;

			OnReportResultChanged?.Invoke();
			RaiseNotify(nameof(ReportData)); // Refresh UI.

			if (dataSource == null)
				return;

			IsLoadedReportResult = false;

			try
			{

				var context = new WidgetDataSourceContext();
				context.HyperStore = this.HyperStore;

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

				ReportData = LoadReportData(Report);
			}
			finally
			{
				IsLoadedReportResult = true;

				OnReportResultChanged?.Invoke();
				RaiseNotify(nameof(ReportData)); // Refresh UI.
			}
		}

		protected virtual string FormatData(object data)
		{
			string appendinx = "";

			if (this.Widget?.AppendPercentageSign == true)
				appendinx += "%";

			return Convert.ToString(data) + appendinx;
		}

		ReportData LoadReportData(Report report)
		{
			var result = new ReportData();

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
					
					var dataSeries = new ReportSeriesData();
					var existingSeries = result.Series.FirstOrDefault(it => it.HeaderTitle == columnTitle);

					if (existingSeries != null)
					{
						dataSeries = existingSeries;
					}
					else
					{
						dataSeries.HeaderTitle = columnTitle;
						resultCategoryRange.Add(columnTitle);
					}

					//for (var rowIndex = 0; rowIndex < rowData.Length; rowIndex++)
					//{
					//	var row = rowData[rowIndex];

					//	var label = row.Template.Title;
					//	var cell = row.Cells[columnIndex];
					//	var data = cell.Values.FirstOrDefault();

					//	ReportSeriesDataItem dataItem = null;
					//	var existingChartItem = dataSeries.Data.FirstOrDefault(it => it.Label == label);
					//	if (dataItem == null)
					//	{
					//		dataItem = new ReportSeriesDataItem
					//		{
					//			CategoryName = columnTitle,
					//			Value = FormatData(data),
					//			Label = label
					//		};
					//		dataSeries.Data.Add(dataItem);
					//	}
					//	else
					//	{
					//		dataItem.Value += Convert.ToUInt16(data.ToString());
					//	}
					//}
					result.Series.Add(dataSeries);
				}
			}
			catch (Exception ex)
			{
				Logger.Instance.Error(typeof(ReportWidgetVm<>), nameof(LoadReportData), ex);
				System.Diagnostics.Debug.Assert(false, ex.Message);
			}

			return result;
		}

	}
}
