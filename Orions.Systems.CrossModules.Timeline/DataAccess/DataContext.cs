using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Timeline
{
	public partial class DataContext
	{
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
	}
}
