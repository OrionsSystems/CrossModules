using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Timeline
{
	public partial class DataContext
	{
		private static readonly int DefaultServerPort = 8585;

		private NetStore _netStore;
		private static readonly Lazy<DataContext> _instance = new Lazy<DataContext>();

		protected string ServerUri { get; private set; }

		protected string MissionInstanceId => Request?.MissionInstanceIds?.First();
		protected string WorkflowInstanceId => Request?.WorkflowInstanceIds?.First();
		protected string AssetId => Request?.AssetIds?.First();

		protected Dictionary<string, IHyperArgsSink> GetStores()
		{
			return new Dictionary<string, IHyperArgsSink> { { ServerUri, _netStore } };
		}

		public CrossModuleVisualizationRequest Request { get; set; }

		public static DataContext Instance => _instance.Value;

		public async Task InitStoreAsync(HyperConnectionSettings connection)
		{
			if (connection == null) throw new ArgumentException(nameof(connection));
			if (string.IsNullOrWhiteSpace(connection.ConnectionUri)) throw new ArgumentException(nameof(connection.ConnectionUri));

			ServerUri = connection.ConnectionUri;
			try
			{
				_netStore = await NetStore.ConnectAsyncThrows(ServerUri);
			}
			catch (Exception ex)
			{
				Logger.Instance.Error("Cannot establish the specified connection", ex.Message);
			}
		}

		public async Task<int> GetServerPortAsync()
		{
			if (_netStore == null) throw new ArgumentException(nameof(_netStore));

			var hlsPort = DefaultServerPort;

			var retrieveConfigurationArgs = new RetrieveConfigurationArgs();

			var result = await _netStore.ExecuteAsync(retrieveConfigurationArgs);

			foreach (var item in result)
			{
				if (item.ComponentConfigType == typeof(StandardsBasedNetStoreServerConfig))
				{
					var config = new StandardsBasedNetStoreServerConfig();
					JsonHelper.Populate(item.Json, config);
					if (config.HttpPort.HasValue) hlsPort = config.HttpPort.Value;
				}
			}

			return hlsPort;
		}

		public async Task<HyperMission> GetHyperMissionAsync()
		{
			if (string.IsNullOrEmpty(Request.MissionId)) throw new ArgumentException(nameof(Request.MissionId));

			var retrieveHyperDocumentArgs = new RetrieveHyperDocumentArgs()
			{
				DocumentId = HyperDocumentId.Create<HyperMission>(Request.MissionId)
			};

			var hyperDocument = await _netStore.ExecuteAsync(retrieveHyperDocumentArgs);

			var mission = hyperDocument.GetPayload<HyperMission>();

			return mission;
		}
	}
}
