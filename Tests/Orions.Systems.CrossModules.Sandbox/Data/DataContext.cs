using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Sandbox.Data
{
	public partial class DataContext
	{
		private NetStore _netStore;
		private static readonly Lazy<DataContext> _instance = new Lazy<DataContext>();

		protected string ServerUri { get; private set; }

		public static DataContext Instance => _instance.Value;

		public NetStore NetStore { get { return _netStore; } }

		public async Task InitStoreAsync(HyperConnectionSettings connection)
		{
			if (connection == null) throw new ArgumentException(nameof(connection));
			if (string.IsNullOrWhiteSpace(connection.ConnectionUri)) throw new ArgumentException(nameof(connection.ConnectionUri));

			ServerUri = connection.ConnectionUri;
			try
			{
				_netStore = await NetStore.ConnectAsyncThrows("http://localhost:5580/Execute"); // Embed node
				//_netStore = await NetStore.ConnectAsyncThrows(ServerUri);
			}
			catch (Exception ex)
			{
				Logger.Instance.Error("Cannot establish the specified connection", ex.Message);
			}
		}
	}
}
