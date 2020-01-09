using Orions.Common;
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
	public abstract class WidgetDataSource : UnifiedBlob
	{
		public WidgetDataSource()
		{
		}

		public abstract Task<HyperMetadataReportResult> GenerateReprotDataAsync(IHyperArgsSink store);
	}

	public class CSVWidgetDataSource : WidgetDataSource
	{
		[UniBrowsable(UniBrowsableAttribute.EditTypes.TextFile)]
		[HelpText("Add report result document", HelpTextAttribute.Priorities.Important)]
		public byte[] Data { get; set; }

		public CSVWidgetDataSource()
		{
		}

		public override async Task<HyperMetadataReportResult> GenerateReprotDataAsync(IHyperArgsSink store)
		{
			var byteArray = this.Data;

			if (byteArray == null)
				return null;

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

			var report = new HyperMetadataReportResult();
			report.Data = result;

			return report;
		}
	}

	public class ReportResultWidgetDataSource : WidgetDataSource
	{
		[HelpText("Add report result document", HelpTextAttribute.Priorities.Important)]
		[HyperDocumentId.DocumentType(typeof(HyperMetadataReportResult))]
		public HyperDocumentId ReportResultId { get; set; }

		public ReportResultWidgetDataSource()
		{
		}

		public override async Task<HyperMetadataReportResult> GenerateReprotDataAsync(IHyperArgsSink store)
		{
			var args = new RetrieveHyperDocumentArgs(this.ReportResultId);
			var doc = await store.ExecuteAsync(args);

			if (args.ExecutionResult.IsNotSuccess)
			{
				return null;
			}

			return doc?.GetPayload<HyperMetadataReportResult>();
		}

	}

	public abstract class ReportBaseWidget : DashboardWidgetBase, IDashboardWidget
	{
		[HelpText("Add the data for this widget to use")]
		public WidgetDataSource DataSource { get; set; } = new ReportResultWidgetDataSource();

		[HelpText("Add category filters separated by comma")]
		public string CategoryFilter { get; set; }

		public ReportBaseWidget()
		{
		}
	}
}
