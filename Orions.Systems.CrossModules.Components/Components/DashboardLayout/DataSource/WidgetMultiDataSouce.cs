using Orions.Infrastructure.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WidgetMultiDataSouce : WidgetDataSource
	{
		public class DataSource
		{
			public string ColumnPrefix { get; set; }
			public WidgetDataSource Source {get;set;}
		}

		public DataSource[] DataSources { get; set; }

		public WidgetMultiDataSouce()
		{

		}

		protected override async Task<IReportResult> OnGenerateFilteredReportResultAsync(WidgetDataSourceContext context)
		{
			List<IReportResult> results = new List<IReportResult>();
			foreach(var item in DataSources ?? Enumerable.Empty<DataSource>())
			{
				var result = await item.Source.GenerateFilteredReportResultAsync(context);
				if (result != null)
				{
					if(!String.IsNullOrWhiteSpace(item.ColumnPrefix))
					{
						var hyperResult = result as HyperMetadataReportResult;
						foreach (var column in hyperResult.ReportData.ColumnsDefinitions)
						{
							column.Title = item.ColumnPrefix + "_" + column.Title;
						}
					}
					results.Add(result);
				}
			}
			var multiResult = new MultiReportResult();
			multiResult.Data = new MultiReportData() { ReportsData = results.ToArray() };
			return multiResult;
		}
	}
}
