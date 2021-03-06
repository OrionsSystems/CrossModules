﻿using Orions.Common;
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
		[HelpText("The CSV data to generate a report from", HelpTextAttribute.Priorities.Mandatory)]
		[UniBrowsable(EditType = UniBrowsableAttribute.EditTypes.TextFile)]
		public byte[] CSVData { get; set; }

		public CSVWidgetDataSource()
		{
		}

		protected override Task<Report> OnGenerateFilteredReportResultAsync(WidgetDataSourceContext context)
		{
			var byteArray = this.CSVData;

			if (byteArray == null)
				return Task.FromResult<Report>(null);

			var reportData = new Report();

			var rowDefList = new List<ReportRowTemplate>();
			var colDefList = new List<ReportColumnTemplate>();
			var dataMap = new Dictionary<int, List<string>>();

			using (var stream = new MemoryStream(byteArray))
			{
				var reader = new StreamReader(stream);

				var lineCount = 0;
				while (!reader.EndOfStream)
				{
					string line = reader.ReadLine();
					//System.Diagnostics.Debug.WriteLine(line);

					if (!String.IsNullOrWhiteSpace(line))
					{
						string[] values = line.Split(',');

						if (values.Any() && lineCount == 0)
						{
							// columns header.
							for (var i = 0; i < values.Length; i++)
							{
								if (i == 0) continue;

								var colDef = new ComputationReportColumnTemplate() { Title = values[i] };
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
									var rowDef = new ComputationReportRowTemplate() { Title = values[i] };
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

				var cells = new List<ReportRowCell>();

				var rowTemplate = reportData.RowsDefinitions[rowIndex - 1];

				var dataCells = columnData.Select(it => new ReportRowCell() { Values = new[] { it } }).ToArray();
				reportData.AddRow(new ReportRow() { Cells = dataCells, Template = rowTemplate });
			}

			return Task.FromResult<Report>(reportData);
		}
	}
}
