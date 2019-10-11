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
		private NetStore _netStore;
		public CrossModuleVisualizationRequest Request { get; set; }
		private static Lazy<DataContext> _instance = new Lazy<DataContext>();

		public static DataContext Instance => _instance.Value;

		public string MissionId { get { return Request?.MissionIds?.First(); }}

		public string[] WorkflowInstanceIds { get { return Request?.WorkflowInstanceIds; } }

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

			var args = new RetrieveContentGroupStatisticsArgs()
			{
				WorkflowInstanceIds = workflowInstanceIds,
				ReportDays = reportDays
			};

			ContentStatistics data = await _netStore.ExecuteAsync(args);

			var result = new ContentStatisticsViewModel()
			{
				ContentDuration = data.ContentDuration,
				ExploitedDuration = data.ExploitedDuration,
				ExploitedPercentage = data.ExploitedPercentage,
				ExploitingDuration = data.ExploitingDuration,
				TaggerExploitationTime = data.TaggerExploitationTime,
				Taggers = data.Taggers,
				Tags = data.Tags,
				TaskDone = data.TaskDone,
				TaskOutstanding = data.TaskOutstanding,
			};

			var todayArgs = new RetrieveContentGroupStatisticsArgs()
			{
				WorkflowInstanceIds = workflowInstanceIds,
				ReportDays = 1
			};

			var today = await _netStore.ExecuteAsync(todayArgs);

			result.TodayExploitedPercentage = today.ExploitedPercentage;
			result.TodayTags = today.Tags;
			result.TodayTaggers = today.Taggers;

			var totalArgs = new RetrieveContentGroupStatisticsArgs()
			{
				WorkflowInstanceIds = workflowInstanceIds,
			};

			var total = await _netStore.ExecuteAsync(totalArgs);

			result.TotalContentDuration = total.ContentDuration;
			result.TotalExploitingDuration = total.ExploitingDuration;
			result.TotalExploitedPercentage = total.ExploitedPercentage;
			result.TotalExploitedPercentage = total.ExploitedPercentage;
			result.TotalTags = total.Tags;
			result.TotalTaggers = total.Taggers;

			return result;
		}

		public async Task<List<ContentProgressViewModel>> GetProgressData(
			string missionInstanceId,
			double reportDays,
			int timeStep)
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

			var args = new RetrieveContentGroupProgressArgs()
			{
				WorkflowInstanceIds = workflowInstanceIds,
				ReportDays = reportDays,
				TimeStep = timeStep
			};

			ContentProgress data = await _netStore.ExecuteAsync(args);

			var result = new List<ContentProgressViewModel>();

			//Grouping
			GroupExploitedDuration(result, data?.ExploitedDuration);
			GroupTotalDuration(result, data?.TotalDuration);
			GroupTasksPerformed(result, data?.TasksPerformed);
			GroupTasksOutstanding(result, data?.TasksOutstanding);
			GroupTasksCompletedPerPeriod(result, data?.TasksCompletedPerPeriod);
			GroupCompletionPercent(result, data?.CompletionPercent);
			GroupSessions(result, data?.Sessions);
			GroupNewTaggers(result, data?.NewTaggers);
			GroupExploitationSaturation(result, data?.ExploitationSaturation);

			return result;
		}

		public async Task<IEnumerable<MissionInstanceItemViewModel>> GetMissionInstances()
		{
			var result = new List<MissionInstanceItemViewModel>();

			if (string.IsNullOrWhiteSpace(MissionId))
			{
				var defaultItems = new List<MissionInstanceItemViewModel>()
				{
					new MissionInstanceItemViewModel() { Label = "All Instances", Id = "0" },
					new MissionInstanceItemViewModel() { Label = "Active Instances", Id = "-1" },
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
				var misionConfig = hyperMissionInstance.MissionConfiguration;

				var item = new MissionInstanceItemViewModel()
				{
					Id = hyperMissionInstance.Id,
					Name = misionConfig.Name,
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

					var data = nodes.Select(it => new WorkflowConfigNodeViewModel()
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
			var statsArgs = new RetrieveHyperWorkflowsStatusesArgs();
			statsArgs.MissionIds = new string[] { missionId };
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

		private void GroupExploitationSaturation(
			List<ContentProgressViewModel> data,
			KeyValuePair<DateTime, double>[] resource)
		{
			if (resource == null) return;

			foreach (var item in resource)
			{
				if (item.Key == null) continue;

				var contentItem = new ContentProgressViewModel()
				{
					Content = "ExploitationSaturation",
					Date = item.Key,
					ExploitationSaturation = item.Value
				};

				data.Add(contentItem);
			}
		}

		private void GroupNewTaggers(
			List<ContentProgressViewModel> data,
			KeyValuePair<DateTime, long>[] resource)
		{
			if (resource == null) return;

			foreach (var item in resource)
			{
				if (item.Key == null) continue;

				var contentItem = new ContentProgressViewModel()
				{
					Content = "NewTaggers",
					Date = item.Key,
					NewTaggers = item.Value
				};

				data.Add(contentItem);
			}
		}

		private void GroupSessions(
			List<ContentProgressViewModel> data,
			KeyValuePair<DateTime, long>[] resource)
		{
			if (resource == null) return;

			foreach (var item in resource)
			{
				if (item.Key == null) continue;

				var contentItem = new ContentProgressViewModel()
				{
					Content = "Sessions",
					Date = item.Key,
					Sessions = item.Value
				};

				data.Add(contentItem);
			}
		}

		private void GroupCompletionPercent(
			List<ContentProgressViewModel> data,
			KeyValuePair<DateTime, double>[] resource)
		{
			if (resource == null) return;

			foreach (var item in resource)
			{
				if (item.Key == null) continue;

				var contentItem = new ContentProgressViewModel()
				{
					Content = "CompletionPercent",
					Date = item.Key,
					CompletionPercent = item.Value
				};

				data.Add(contentItem);
			}
		}

		private void GroupTasksCompletedPerPeriod(
			List<ContentProgressViewModel> data,
			KeyValuePair<DateTime, int>[] resource)
		{
			if (resource == null) return;

			foreach (var item in resource)
			{
				if (item.Key == null) continue;

				var contentItem = new ContentProgressViewModel()
				{
					Content = "TasksCompletedPerPeriod",
					Date = item.Key,
					TasksCompletedPerPeriod = item.Value
				};

				data.Add(contentItem);
			}
		}

		private void GroupTasksOutstanding(
			List<ContentProgressViewModel> data,
			KeyValuePair<DateTime, int>[] resource)
		{
			if (resource == null) return;

			foreach (var item in resource)
			{
				if (item.Key == null) continue;

				var contentItem = new ContentProgressViewModel()
				{
					Content = "TasksOutstanding",
					Date = item.Key,
					TasksOutstanding = item.Value
				};

				data.Add(contentItem);
			}
		}

		private void GroupTasksPerformed(
			List<ContentProgressViewModel> data,
			KeyValuePair<DateTime, int>[] resource)
		{
			if (resource == null) return;

			foreach (var item in resource)
			{
				if (item.Key == null) continue;

				var contentItem = new ContentProgressViewModel()
				{
					Content = "TasksPerformed",
					Date = item.Key,
					TasksPerformed = item.Value
				};

				data.Add(contentItem);
			}
		}

		private void GroupTotalDuration(
			List<ContentProgressViewModel> data,
			KeyValuePair<DateTime, TimeSpan>[] resource)
		{
			if (resource == null) return;

			foreach (var item in resource)
			{
				if (item.Key == null) continue;

				var contentItem = new ContentProgressViewModel()
				{
					Content = "TotalDuration",
					Date = item.Key,
					TotalDuration = item.Value.TotalHours
				};

				data.Add(contentItem);
			}
		}

		private void GroupExploitedDuration(
			List<ContentProgressViewModel> data,
			KeyValuePair<DateTime, TimeSpan>[] resource)
		{
			if (resource == null) return;

			foreach (var item in resource)
			{
				if (item.Key == null) continue;

				var contentItem = new ContentProgressViewModel()
				{
					Content = "ExploitedDuration",
					Date = item.Key,
					ExploitedDuration = item.Value.TotalHours
				};

				data.Add(contentItem);
			}
		}
	}
}
