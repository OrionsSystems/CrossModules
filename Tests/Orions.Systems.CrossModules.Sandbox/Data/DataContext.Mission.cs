using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Sandbox.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Sandbox.Data
{
	public partial class DataContext
	{
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


		public async Task<IEnumerable<MissionViewModel>> GetMissionsAsync(
			int pageNumber = 0,
			int pageSize = 100,
			bool onlyActive = false)
		{

			var retrieveHyperDocumentArgs = new FindHyperDocumentsArgs()
			{
				Skip = pageNumber * pageSize,
				Limit = pageSize,
			};

			retrieveHyperDocumentArgs.SetDocumentType(typeof(HyperMission));

			var condition = new MultiScopeCondition();

			if (onlyActive)
			{
				//var conditionOr = new MultiScopeCondition()
				//{
				//	Mode = AndOr.Or
				//};
				//conditionOr.AddCondition(Assist.GetPropertyName((HyperMission i) => i.Archived), false);
				//conditionOr.AddCondition(Assist.GetPropertyName((HyperMission i) => i.Archived), false, ScopeCondition.Comparators.DoesNotExist);
				//condition.AddCondition(conditionOr);
			}

			retrieveHyperDocumentArgs.DescriptorConditions.AddCondition(condition);

			var results = await _netStore.ExecuteAsync(retrieveHyperDocumentArgs);

			var missionVms = new List<MissionViewModel>();

			foreach (var hyperDocument in results)
			{
				var mission = hyperDocument.GetPayload<HyperMission>();

				var missionVM = new MissionViewModel
				{
					MissionId = mission.Id,
					Mission = mission,
					Name = mission.Name,
					Document = hyperDocument,
					IsActive = !mission.Archived,
				};

				missionVms.Add(missionVM);
			}


			var getMissionStatusTasks = new List<IHyperArgs<HyperMissionInstanceStatus>>();

			var listRetrieveMissionStatusArgs = new List<Tuple<string, GetHyperMissionStatusArgs>>();

            foreach (var mission in missionVms)
			{
				var args = new GetHyperMissionStatusArgs()
				{
					MissionId = mission.Mission.Id
				};


				var getMissionStatusTask = _netStore.ExecuteAsync(args);

				getMissionStatusTasks.Add(getMissionStatusTask);
				listRetrieveMissionStatusArgs.Add(
									new Tuple<string, GetHyperMissionStatusArgs>(
										mission.MissionId,
										args
									));
			}

			await Task.WhenAll(getMissionStatusTasks.Select(x => x.AsTask()).ToList());

			foreach (var missionStatusArgs in listRetrieveMissionStatusArgs)
			{
				// TODO - better error handling
				if (missionStatusArgs.Item2.ExecutionResult.IsFailure) continue;
				//throw new InvalidOperationException(retrieveAssetsArgs.ExecutionResult.ToString());

				HyperMissionInstanceStatus instance = missionStatusArgs.Item2.Result;

				var item = missionVms.FirstOrDefault(it => it.MissionId == missionStatusArgs.Item1);
				item.InstanceStatus = instance;
				item.InstanceDescription = instance.ToString();

                item.Status = ManageMissionInstanseStatusStyle(instance);
            }

            missionVms = missionVms.OrderBy(it => it.Status).ToList();


			return missionVms;
		}

		private string ManageMissionInstanseStatusStyle(HyperMissionInstanceStatus instance)
		{

			var result = "Inactive";

			if (instance == null) return result;

			if (instance.State == HyperMissionInstanceStatus.States.Loaded)
			{
				if (instance.PhaseStatuses != null && instance.PhaseStatuses.Any(it => it.LastTaskProcessedUTC.HasValue && it.LastTaskProcessedUTC.Value > DateTime.UtcNow - TimeSpan.FromMinutes(3)))
				{
					return "Operating";
				}

				return "Active";
			}

			if (instance.State == HyperMissionInstanceStatus.States.Loading)
			{
				return "Loading";
			}

			return result;
		}
	}
}
