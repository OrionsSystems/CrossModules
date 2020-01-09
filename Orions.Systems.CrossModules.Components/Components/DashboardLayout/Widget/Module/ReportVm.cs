using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class ReportVm : BlazorVm
	{
		public IHyperArgsSink HyperStore { get; private set; }

		public HyperMetadataReportResult Report { get; private set; }

		public bool IsLoadedReportResult { get; set; }

		public string ReportName { get { return Report?.Report?.Name; } }

		public bool ReportHasName { get { return !string.IsNullOrWhiteSpace(ReportName); } }

		public ReportVm()
		{

		}

		public async Task InitStoreAsync(HyperConnectionSettings connection)
		{
			if (connection != null)
			{
				try
				{
					//HyperStore = await NetStore.ConnectAsyncThrows("http://localhost:5580/Execute");
					HyperStore = await NetStore.ConnectAsyncThrows(connection.ConnectionUri);
				}
				catch (Exception ex)
				{
					Logger.Instance.Error("Cannot establish the specified connection", ex.Message);
				}
			}
		}

		public void InitStore(IHyperArgsSink netStore)
		{
			if (netStore != null)
			{
				HyperStore = netStore;
			}
		}

		public async Task LoadReportResultData(WidgetDataSource dataSource)
		{
			var reportResult = await dataSource.GenerateReprotDataAsync(this.HyperStore);
			if (reportResult == null)
			{
				Logger.Instance.Error("Cannot load report result");
				IsLoadedReportResult = true;
				return;
			}

			Report = reportResult;
			IsLoadedReportResult = true;
		}

		public ReportChartData LoadReportLineChartData(string filter)
		{
			var result = new ReportChartData();

			if (Report == null) return result;

			var categoryFilters = new List<string>();
			if (!string.IsNullOrWhiteSpace(filter)) {
				categoryFilters = filter.Split(',').Select(it => it.Trim()).ToList();
			}

			var categories = Report.Data.ColumnsDefinitions.Select(it => it.Title).ToList();
			var rowsDef = Report.Data.RowsDefinitions.ToList();

			result.Categories.AddRange(categories);

			var rowData = Report.Data.RowsCells;

			for (var i = 0; i < categories.Count; i++)
			{
				var categoryTitle = categories[i];

				if (categoryFilters.Any() && !categoryFilters.Contains(categoryTitle)) {
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

					chartItem.DatePosition = ParseTimePosition(timeEl);
					chartItem.StreamPosition = ParseStreamPosition(timeEl);
					chartSeries.Data.Add(chartItem);
				}

				result.Series.Add(chartSeries);
			}

			return result;
		}

		public string ParseStreamPosition(string timeEl)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(timeEl) && timeEl.Contains("(") && timeEl.Contains(")"))
				{
					var timeStr = timeEl.Substring(timeEl.LastIndexOf("(") + 1, timeEl.LastIndexOf(")") - timeEl.LastIndexOf("(") - 1);

					return timeEl.Substring(0, timeEl.LastIndexOf("("));
				}
			}
			catch (Exception ex) { Console.WriteLine(ex.Message); }

			throw new Exception("Cannot parse row defenition title");
		}

		public DateTime ParseTimePosition(string timeEl)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(timeEl) && timeEl.Contains("(") && timeEl.Contains(")"))
				{
					var timeStr = timeEl.Substring(timeEl.LastIndexOf("(") + 1, timeEl.LastIndexOf(")") - timeEl.LastIndexOf("(") - 1);

					return DateTime.ParseExact(
						timeStr,
						"MM/dd/yyyy h:mm:ss tt",
						System.Globalization.CultureInfo.InvariantCulture);
				}
			}
			catch (Exception ex) { Console.WriteLine(ex.Message); }

			throw new Exception("Cannot parse row defenition title");
		}

	}
}
