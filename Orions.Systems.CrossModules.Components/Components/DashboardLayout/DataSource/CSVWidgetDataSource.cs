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

		protected override Task<Report> OnGenerateFilteredReportResultAsync(WidgetDataSourceContext context)
		{
			var byteArray = this.Data;

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

				var cells = new List<ReportOutputCell>();

				var dataCells = columnData.Select(it => new ReportOutputCell() { Values = new[] { it } }).ToArray();
				reportData.AddRow(new ReportRow() { Cells = dataCells });
			}

			// We want to filter at the end, to ensure both data and definitions are synchronized filtered.
			if (context.DynamicFilter != null)
				reportData.FilterWith(context.DynamicFilter);

			return Task.FromResult<Report>(reportData);
		}
	}
}
