using Orions.Common;
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
	}
}
