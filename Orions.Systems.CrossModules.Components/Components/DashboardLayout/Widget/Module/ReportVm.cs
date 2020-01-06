﻿using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class ReportVm : BlazorVm
	{
		public IHyperArgsSink HyperStore { get; private set; }

		public HyperMetadataReportResult Report { get; private set; }

		public bool IsLoadedReportResult { get; set; }

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

					//parse date 00:00:00 (11/17/2019 9:30:00 AM)
					try
					{
						if (!string.IsNullOrWhiteSpace(timeEl) && timeEl.Contains("(") && timeEl.Contains(")"))
						{
							var timeStr = timeEl.Substring(timeEl.LastIndexOf("(") + 1, timeEl.LastIndexOf(")") - timeEl.LastIndexOf("(") - 1);

							chartItem.DatePosition = DateTime.ParseExact(
								timeStr,
								"MM/dd/yyyy h:mm:ss tt",
								System.Globalization.CultureInfo.InvariantCulture);

							chartItem.StreamPosition = timeEl.Substring(0, timeEl.LastIndexOf("("));
						}
					}
					catch (Exception ex) { Console.WriteLine(ex.Message); }
					chartSeries.Data.Add(chartItem);
				}

				result.Series.Add(chartSeries);
			}

			return result;
		}
	}
}
