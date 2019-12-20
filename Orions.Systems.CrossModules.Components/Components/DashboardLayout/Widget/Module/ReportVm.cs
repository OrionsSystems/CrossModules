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

		public void InitStore(NetStore netStore)
		{
			if (netStore != null)
			{
				HyperStore = netStore;
			}
		}

		public async Task LoadReportResultData(
			string reportResultId = null,
			string metadatasetId = null)
		{
			var results = await FetchReportResultList(reportResultId, metadatasetId);

			var lastResult = results.OrderByDescending(it => it.CreatedAtUTC).FirstOrDefault();

			if (lastResult == null) return;

			var data = await LoadResult(lastResult);

			IsLoadedReportResult = true;

			Report = data;
		}

		private async Task<List<HyperMetadataReportResult>> FetchReportResultList(
			string reportResultId = null,
			string metadatasetId = null)
		{
			var results = new List<HyperMetadataReportResult>();

			var args = new FindHyperDocumentsArgs(typeof(HyperMetadataReportResult)) { RetrievePayload = false };
			if (!string.IsNullOrWhiteSpace(reportResultId))
				args.DescriptorConditions.AddCondition(nameof(HyperMetadataReportResult.Id), reportResultId);

			if (!string.IsNullOrWhiteSpace(metadatasetId))
				args.DescriptorConditions.AddCondition(nameof(HyperMetadataReportResult.MetadataSetId), metadatasetId);

			var docs = await HyperStore.ExecuteAsync(args);

			if (docs == null)
				return results;

			foreach (var result in docs.Select(it => it.GetPayload<HyperMetadataReportResult>(true)))
			{
				results.Add(result);
			}

			return results;
		}

		private async Task<HyperMetadataReportResult> LoadResult(HyperMetadataReportResult result)
		{
			var args = new RetrieveHyperDocumentArgs(result.GetDocumentId());
			var doc = await HyperStore.ExecuteAsync(args);

			if (args.ExecutionResult.IsNotSuccess)
			{
				Logger.Instance.Error("Cannot load report result");
				return null;
			}

			result = doc?.GetPayload<HyperMetadataReportResult>();

			return result;
		}


	}
}
