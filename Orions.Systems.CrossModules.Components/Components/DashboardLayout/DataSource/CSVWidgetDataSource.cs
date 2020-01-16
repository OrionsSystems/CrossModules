using Orions.Common;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class CSVWidgetDataSource : WidgetDataSource
	{
		[UniBrowsable(UniBrowsableAttribute.EditTypes.TextFile)]
		[HelpText("Add report result document", HelpTextAttribute.Priorities.Important)]
		public byte[] Data { get; set; }

		public override bool SupportsDynamicFiltration => true;

		public CSVWidgetDataSource()
		{
		}

		public override async Task<IReportResult> GenerateFilteredReportResultAsync(WidgetDataSourceContext context)
		{
			var byteArray = this.Data;

			if (byteArray == null)
				return null;

			var reportData = new ReportData();

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

							reportData.ColumnsDefinitions = colDefList.ToArray();

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

			reportData.ColumnsDefinitions = colDefList.ToArray();
			reportData.RowsDefinitions = rowDefList.ToArray();

			var item1 = dataMap.Values;

			foreach (KeyValuePair<int, List<string>> item in dataMap)
			{
				var rowIndex = item.Key;
				var columnData = item.Value;

				var cells = new List<ReportDataCell>();
				var dataCell = columnData.Select(it => new ReportDataCell() { Values = new[] { it } }).ToArray();
				reportData.AddRow(dataCell);
			}

			// We want to filter at the end, to ensure both data and definitions are synchronized filtered.
			if (context.DynamicFilter != null)
				reportData.FilterWith(context.DynamicFilter);

			var report = new HyperMetadataReportResult();
			report.Data = reportData;

			return report;
		}
	}
}
