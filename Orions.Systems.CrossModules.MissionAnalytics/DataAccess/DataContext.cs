using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.MissionAnalytics.Model;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class DataContext
	{
		private const int RoundingFactor = 2;
		private const MidpointRounding RoundingMethod = MidpointRounding.AwayFromZero;

		private NetStore _netStore;

		public CrossModuleVisualizationRequest Request { get; set; }

		private static readonly Lazy<DataContext> _instance = new Lazy<DataContext>();
		public static DataContext Instance => _instance.Value;

		public string MissionId => Request?.MissionIds?.First();

		public string[] WorkflowInstanceIds => Request?.WorkflowInstanceIds;

		public async Task InitStoreAsync(HyperConnectionSettings connection)
		{
			if (connection == null) throw new ArgumentException(nameof(connection));
			if (string.IsNullOrWhiteSpace(connection.ConnectionUri)) throw new ArgumentException(nameof(connection.ConnectionUri));

			try
			{
				_netStore = await NetStore.ConnectAsyncThrows(connection.ConnectionUri);
			}
			catch (Exception ex)
			{
				Logger.Instance.Error("Cannot establish the specified connection", ex.Message);
			}
		}

		public async Task<ContentStatisticsViewModel> GetStatisticsData(
			string missionInstance,
			double reportDays)
		{
			var workflowInstanceIds = WorkflowInstanceIds;
			if (!string.IsNullOrEmpty(missionInstance) && missionInstance != "0" && missionInstance != "-1")
			{
				workflowInstanceIds = await FindWorkflowInstanceIds(missionInstance);
			}
			if (!string.IsNullOrEmpty(missionInstance) && missionInstance == "-1")
			{
				// load all active instances
				workflowInstanceIds = await FindActiveWorkflowInstanceIds(MissionId);
			}

			var model = new ContentStatisticsViewModel();

			// Get stats for the selected time range
			var args = new RetrieveContentGroupStatisticsArgs
			{
				WorkflowInstanceIds = workflowInstanceIds,
				ReportDays = reportDays
			};
			var data = await _netStore.ExecuteAsync(args);

			model.ContentDuration = data.ContentDuration;
			model.ExploitedDuration = data.ExploitedDuration;
			model.ExploitedPercentage = data.ExploitedPercentage;
			model.ExploitingDuration = data.ExploitingDuration;
			model.TaggerExploitationTime = data.TaggerExploitationTime;
			model.Taggers = data.Taggers;
			model.Tags = data.Tags;
			model.TaskDone = data.TaskDone;
			model.TaskOutstanding = data.TaskOutstanding;

			// Get stats for the last 24 hours
			args = new RetrieveContentGroupStatisticsArgs
			{
				WorkflowInstanceIds = workflowInstanceIds,
				ReportDays = 1
			};
			data = await _netStore.ExecuteAsync(args);

			model.TodayExploitedPercentage = data.ExploitedPercentage;
			model.TodayTags = data.Tags;
			model.TodayTaggers = data.Taggers;

			// Get all time stats
			args = new RetrieveContentGroupStatisticsArgs
			{
				WorkflowInstanceIds = workflowInstanceIds,
			};
			data = await _netStore.ExecuteAsync(args);

			model.TotalContentDuration = data.ContentDuration;
			model.TotalExploitingDuration = data.ExploitingDuration;
			model.TotalExploitedPercentage = data.ExploitedPercentage;
			model.TotalExploitedPercentage = data.ExploitedPercentage;
			model.TotalTags = data.Tags;
			model.TotalTaggers = data.Taggers;

			return model;
		}

		public async Task<ContentProgressViewModel> GetProgressData(
			string missionInstanceId,
			double reportDays,
			int timeStep, 
			string formatString)
		{
			var workflowInstanceIds = WorkflowInstanceIds;
			if (!string.IsNullOrEmpty(missionInstanceId) && missionInstanceId != "0" && missionInstanceId != "-1")
			{
				workflowInstanceIds = await FindWorkflowInstanceIds(missionInstanceId);
			}
			if (!string.IsNullOrEmpty(missionInstanceId) && missionInstanceId == "-1")
			{
				// load all active instances
				workflowInstanceIds = await FindActiveWorkflowInstanceIds(MissionId);
			}

			var args = new RetrieveContentGroupProgressArgs
			{
				WorkflowInstanceIds = workflowInstanceIds,
				ReportDays = reportDays,
				TimeStep = timeStep
			};
			var data = await _netStore.ExecuteAsync(args);

			var model = new ContentProgressViewModel
			{
				ExploitedDuration = GetModel(data?.ExploitedDuration, formatString),
				TotalDuration = GetModel(data?.TotalDuration, formatString),
				TasksPerformed = GetModel(data?.TasksPerformed, formatString),
				TasksOutstanding = GetModel(data?.TasksOutstanding, formatString),
				TasksCompletedPerPeriod = GetModel(data?.TasksCompletedPerPeriod, formatString),
				CompletionPercent = GetModel(data?.CompletionPercent, formatString),
				Sessions = GetModel(data?.Sessions, formatString),
				NewTaggers = GetModel(data?.NewTaggers, formatString),
				ExploitationSaturation = GetModel(data?.ExploitationSaturation, formatString)
			};

			return model;
		}

		public async Task<IEnumerable<MissionInstanceItemViewModel>> GetMissionInstances()
		{
			var result = new List<MissionInstanceItemViewModel>();

			if (string.IsNullOrWhiteSpace(MissionId))
			{
				var defaultItems = new List<MissionInstanceItemViewModel>
				{
					new MissionInstanceItemViewModel { Label = "All Instances", Id = "0" },
					new MissionInstanceItemViewModel { Label = "Active Instances", Id = "-1" },
				};

				result.AddRange(defaultItems);

				return result;
			}

			var args = new FindHyperDocumentsArgs(true);
			args.SetDocumentType(typeof(HyperMissionInstance));
			args.DescriptorConditions.AddCondition("MissionId", MissionId);

			var docs = await _netStore.ExecuteAsync(args);

			foreach (var doc in docs)
			{
				var hyperMissionInstance = doc.GetPayload<HyperMissionInstance>();
				var missionConfiguration = hyperMissionInstance.MissionConfiguration;

				var item = new MissionInstanceItemViewModel
				{
					Id = hyperMissionInstance.Id,
					Name = missionConfiguration.Name,
					RunAtUTC = hyperMissionInstance.RunAtUTC,
					StopAtUTC = hyperMissionInstance.StopAtUTC,
					Label = $"{hyperMissionInstance.RunAtUTC} till {hyperMissionInstance.StopAtUTC}"
				};
				result.Add(item);
			}

			return result;
		}

		public async Task<List<WorkflowConfigNodeViewModel>> FindNodes(
			string missionId)
		{
			var result = new List<WorkflowConfigNodeViewModel>();

			if (string.IsNullOrEmpty(missionId)) return result;

			var retrieveHyperDocumentArgs = new FindHyperDocumentsArgs(true);
			retrieveHyperDocumentArgs.SetDocumentType(typeof(HyperMissionInstance));
			//retrieveHyperDocumentArgs.DocumentConditions.AddCondition("_id", missionInstanceId);
			retrieveHyperDocumentArgs.DescriptorConditions.AddCondition("MissionId", missionId);

			var docs = await _netStore.ExecuteAsync(retrieveHyperDocumentArgs);

			var missionInstances = new List<HyperMissionInstance>();

			foreach (var doc in docs)
			{
				var missionInstance = doc.GetPayload<HyperMissionInstance>();
				if (missionInstance != null) missionInstances.Add(missionInstance);
			}

			foreach (var missionInstance in missionInstances)
			{
				var config = missionInstance.MissionConfiguration;
				if (config == null) continue;

				foreach (var phase in config.Phases)
				{
					var workflowConfig = phase.HyperWorkflowConfigurationId;
					var wId = workflowConfig.Id;

					var workflowConfigArgs = new RetrieveHyperDocumentArgs(workflowConfig);

					var hyperDocument = await _netStore.ExecuteAsync(workflowConfigArgs);
					if (hyperDocument == null) continue;

					var workflowConfiguration = hyperDocument.GetPayload<HyperWorkflow>();

					var nodes = workflowConfiguration.Nodes;
					var workflowId = workflowConfiguration.Id;

					var data = nodes.Select(it => new WorkflowConfigNodeViewModel
					{
						Id = it.Id,
						Name = it.Name
					}).ToList();

					result.AddRange(data);
				}
			}

			return result;
		}

		private async Task<string[]> FindActiveWorkflowInstanceIds(string missionId)
		{
			if (string.IsNullOrEmpty(missionId)) return new string[] { };

			// Pull statuses
			var statsArgs = new RetrieveHyperWorkflowsStatusesArgs { MissionIds = new[] { missionId } };
			var statuses = await _netStore.ExecuteAsync(statsArgs);

			if (statuses == null || !statuses.Any()) return new string[] { };

			var data = new List<HyperWorkflowInstance>();

			// load all workflow instances
			var args = new FindHyperDocumentsArgs(true);
			args.SetDocumentType(typeof(HyperWorkflowInstance));
			args.DescriptorConditions.AddCondition("MissionId", missionId);

			HyperWorkflowInstance[] instances = await FindHyperDocumentsArgs.FindAsync<HyperWorkflowInstance>(_netStore, args);

			foreach (var status in statuses)
			{
				var activeInstance = instances.FirstOrDefault(it => it.Id == status.WorkflowInstanceId);
				if (activeInstance == null) continue;

				data.Add(activeInstance);
			}

			return data.Select(it => it.Id).ToArray();
		}

		private async Task<string[]> FindWorkflowInstanceIds(string missionInstanceId)
		{
			var args = new FindHyperDocumentsArgs(true);
			args.SetDocumentType(typeof(HyperWorkflowInstance));
			args.DescriptorConditions.AddCondition("MissionInstanceId", missionInstanceId);

			HyperWorkflowInstance[] instances = await FindHyperDocumentsArgs.FindAsync<HyperWorkflowInstance>(_netStore, args);

			return instances.Select(it => it.Id).ToArray();
		}

		private List<KeyValueModel> GetModel(
			KeyValuePair<DateTime, TimeSpan>[] resource,
			string formatString)
		{
			if (resource == null) return new List<KeyValueModel>();

			var model = new List<KeyValueModel>();

			foreach (var item in resource)
			{
				model.Add(new KeyValueModel
				{
					Key = item.Key.ToLocalTime().ToString(formatString),
					Value = Math.Round(item.Value.TotalHours, RoundingFactor, RoundingMethod)
				});
			}

			return model;
		}

		private List<KeyValueModel> GetModel(
			KeyValuePair<DateTime, int>[] resource,
			string formatString)
		{
			if (resource == null) return new List<KeyValueModel>();

			var model = new List<KeyValueModel>();

			foreach (var item in resource)
			{
				model.Add(new KeyValueModel
				{
					Key = item.Key.ToLocalTime().ToString(formatString),
					Value = item.Value
				});
			}

			return model;
		}

		private List<KeyValueModel> GetModel(
			KeyValuePair<DateTime, double>[] resource, 
			string formatString)
		{
			if (resource == null) return new List<KeyValueModel>();

			var model = new List<KeyValueModel>();

			foreach (var item in resource)
			{
				model.Add(new KeyValueModel
				{
					Key = item.Key.ToLocalTime().ToString(formatString),
					Value = Math.Round(item.Value, RoundingFactor, RoundingMethod)
				});
			}

			return model;
		}

		private List<KeyValueModel> GetModel(
			KeyValuePair<DateTime, long>[] resource,
			string formatString)
		{
			if (resource == null) return new List<KeyValueModel>();

			var model = new List<KeyValueModel>();

			foreach (var item in resource)
			{
				model.Add(new KeyValueModel
				{
					Key = item.Key.ToLocalTime().ToString(formatString),
					Value = item.Value
				});
			}

			return model;
		}
	}
}
