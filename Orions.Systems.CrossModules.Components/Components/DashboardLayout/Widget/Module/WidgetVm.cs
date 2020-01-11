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
	/// <summary>
	/// Optional generic class helps assign a specialized type for the Widget class.
	/// </summary>
	public class WidgetVm<WidgetType> : WidgetVm
		where WidgetType : IDashboardWidget
	{
		public new WidgetType Widget
		{
			get => (WidgetType)base.Widget;
			set => base.Widget = value;
		}

		public WidgetVm()
		{
		}
	}

	public class WidgetVm : BlazorVm
	{
		IHyperArgsSink _hyperStore = null;
		public IHyperArgsSink HyperStore
		{
			get
			{
				return _hyperStore;
			}

			set
			{
				if (value != null)
					_hyperStore = value;
			}
		}

		public IDashboardWidget Widget { get; set; }

		public IReportResult Report { get; private set; }

		public bool IsLoadedReportResult { get; set; }

		public string ReportName { get { return Report?.Name; } }

		public bool ReportHasName { get { return !string.IsNullOrWhiteSpace(ReportName); } }

		public WidgetVm()
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

		public async Task LoadReportResultData(WidgetDataSource dataSource)
		{
			var context = new WidgetDataSourceContext();
			context.HyperStore = this.HyperStore;

			var reportResult = await dataSource.GenerateReportResultAsync(context);
			if (reportResult == null)
			{
				Logger.Instance.Error("Cannot load report result");
				IsLoadedReportResult = true;
				return;
			}

			Report = reportResult;
			IsLoadedReportResult = true;
		}

		public ReportChartData LoadReportChartData(string filter)
		{
			var result = new ReportChartData();

			if (Report == null) return result;

			var categoryFilters = new List<string>();
			if (!string.IsNullOrWhiteSpace(filter))
			{
				categoryFilters = filter.Split(',').Select(it => it.Trim()).ToList();
			}

			var categories = Report.Data.ColumnsDefinitions.Select(it => it.Title).ToList();
			var rowsDef = Report.Data.RowsDefinitions.ToList();

			result.Categories.AddRange(categories);

			var rowData = Report.Data.RowsCells;

			for (var i = 0; i < categories.Count; i++)
			{
				var categoryTitle = categories[i];

				if (categoryFilters.Any() && !categoryFilters.Contains(categoryTitle))
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
							chartItem.DatePosition = ParseTimePosition(timeEl);
							chartItem.StreamPosition = ParseStreamPosition(timeEl);
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
			catch (Exception) { throw; }

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
			catch (Exception) { throw; }

			throw new Exception("Cannot parse row defenition title");
		}

	}
}
