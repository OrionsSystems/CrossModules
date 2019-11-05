using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.TestGridProperties.Data
{
	public class DataContext
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
				_netStore = await NetStore.ConnectAsyncThrows(ServerUri);
			}
			catch (Exception ex)
			{
				Logger.Instance.Error("Cannot establish the specified connection", ex.Message);
			}
		}

		public async Task<HyperMissionPhase> GetMissionPhaseAsync(
			string missionId,
			string phaseId,
			CancellationToken cancellationToken = default(CancellationToken)) 
		{
			if (_netStore == null) throw new ApplicationException("Node does not exist.");
			if (string.IsNullOrWhiteSpace(missionId)) throw new ArgumentException(nameof(missionId));
			if (string.IsNullOrWhiteSpace(phaseId)) throw new ArgumentException(nameof(phaseId));

			var retrieveHyperDocumentArgs = new FindHyperDocumentsArgs();
			retrieveHyperDocumentArgs.SetDocumentType(typeof(HyperMission));
			retrieveHyperDocumentArgs.DocumentConditions.AddCondition("_id", missionId);

			await _netStore.ExecuteAsync(retrieveHyperDocumentArgs);
			var hyperDocument = retrieveHyperDocumentArgs.Result.FirstOrDefault();
			if (hyperDocument == null) throw new ApplicationException("Missing mission");

			var mission = hyperDocument.GetPayload<HyperMission>();

			foreach (var phase in mission.Phases)
			{
				if (phase?.Id == phaseId) return phase;
			}

			throw new ApplicationException("Missing mission phase");
		}

		public async Task<HyperMission> GetMissionAsync(
		  string missionId,
		  CancellationToken cancellationToken = default(CancellationToken))
		{
			if (_netStore == null) throw new ApplicationException("Node does not exist.");
			if (string.IsNullOrWhiteSpace(missionId)) throw new ArgumentException(nameof(missionId));

			var retrieveHyperDocumentArgs = new RetrieveHyperDocumentArgs()
			{
				DocumentId = HyperDocumentId.Create<HyperMission>(missionId)
			};

			var hyperDocument = await _netStore.ExecuteAsync(retrieveHyperDocumentArgs);

			var mission = hyperDocument.GetPayload<HyperMission>();

			if (mission.Id == missionId)
			{
				return mission;
			}

			throw new ApplicationException("Missing mission");
		}
	}
}
