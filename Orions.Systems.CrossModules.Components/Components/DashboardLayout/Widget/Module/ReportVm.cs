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

			if (dataSource is CSVWidgetDataSource csvSource)
			{
				var reportData = GetCSVWidgetReport(csvSource);
				Report = new HyperMetadataReportResult();
				Report.Data = reportData;
				IsLoadedReportResult = true;
			}
			else if (dataSource is ReportResultWidgetDataSource reportResultSource)
			{
				await this.LoadReportResultData(reportResultSource.ReportResultId);
			}
			else
			{
				throw new NotImplementedException("Data source not recognized");
			}
		}

		public async Task LoadReportResultData(HyperDocumentId reportResultId)
		{
			var args = new RetrieveHyperDocumentArgs(reportResultId);
			var doc = await HyperStore.ExecuteAsync(args);

			if (args.ExecutionResult.IsNotSuccess)
			{
				Logger.Instance.Error("Cannot load report result");
				IsLoadedReportResult = true;
				return;
			}

			Report = doc?.GetPayload<HyperMetadataReportResult>();

			IsLoadedReportResult = true;
		}

		public ReportChartData LoadReportLineChartData()
		{
			var result = new ReportChartData();

			if (Report == null) return result;

			var categories = Report.Data.ColumnsDefinitions.Select(it => it.Title).ToList();
			var rowsDef = Report.Data.RowsDefinitions.ToList();

			result.Categories.AddRange(categories);

			var rowData = Report.Data.RowsCells;

			for (var i = 0; i < categories.Count; i++)
			{
				var chartSeries = new ReportSeriesChartData();
				chartSeries.Name = categories[i];

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

		private ReportData GetCSVWidgetReport(CSVWidgetDataSource csvSource)
		{
			var byteArray = csvSource.Data;

			if (byteArray == null) return null;

			var result = new ReportData();

			var rowDefList = new List<ReportRow>();
			var colDefList = new List<ReportColumn>();
			var dataMap = new Dictionary<int, List<string>>();

			using (var stream = new MemoryStream(byteArray))
			{
				var reader = new StreamReader(stream);

				var lineCount = 0;
				while (!reader.EndOfStream)
				{
					string line = reader.ReadLine();
					Console.WriteLine(line);

					if (!String.IsNullOrWhiteSpace(line))
					{
						string[] values = line.Split(',');

						if (values.Any() && lineCount == 0)
						{
							// columns header.
							for (var i = 0; i < values.Length; i++)
							{
								if (i == 0) continue;
								var colDef = new ReportColumn() { Title = values[i] };
								colDefList.Add(colDef);
							}

							result.ColumnsDefinitions = colDefList.ToArray();

						}
						else
						{
							var dataList = new List<string>();
							for (var i = 0; i < values.Length; i++)
							{
								if (i == 0)
								{
									var rowDef = new ReportRow() { Title = values[i] };
									rowDefList.Add(rowDef);
									continue;
								};

								dataList.Add(values[i]);
							}

							dataMap.Add(lineCount, dataList);
						}
					}

					lineCount++;
				}
			}

			result.ColumnsDefinitions = colDefList.ToArray();
			result.RowsDefinitions = rowDefList.ToArray();

			var item1 = dataMap.Values;

			foreach (var item in dataMap)
			{
				var rowIndex = item.Key;
				var columnData = item.Value;

				var cells = new List<ReportDataCell>();
				var dataCell = columnData.Select(it => new ReportDataCell() { Values = new[] { it } }).ToArray();
				result.AddRow(dataCell);

			}

			return result;
		}
	}
}
