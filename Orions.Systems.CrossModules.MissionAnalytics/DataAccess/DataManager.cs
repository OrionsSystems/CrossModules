using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Orions.Systems.CrossModules.MissionAnalytics.Model;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class DataManager
	{
		private const int RoundingFactor = 2;
		private const MidpointRounding RoundingMethod = MidpointRounding.AwayFromZero;
		private const string TimeSpanFormatString = "c"; 

		private NetStore _netStore;
		public CrossModuleVisualizationRequest Request { get; }

		public DataManager(
			CrossModuleVisualizationRequest request)
		{
			if (request == null) throw new ArgumentException(nameof(request));
			if (string.IsNullOrWhiteSpace(request.MissionId)) throw new ArgumentException(nameof(request.MissionId));

			Request = request;
		}

		public async Task InitStoreAsync(
			HyperConnectionSettings connection)
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
			string missionInstanceId,
			double reportDays)
		{
			if (string.IsNullOrWhiteSpace(missionInstanceId)) throw new ArgumentException(nameof(missionInstanceId));
			if (reportDays < 0) throw new ArgumentException(nameof(missionInstanceId));

			var workflowInstanceIds = await GetWorkflowInstanceIdsAsync(missionInstanceId);
			if (workflowInstanceIds == null || workflowInstanceIds.Length == 0) return new ContentStatisticsViewModel();

			// Get stats for the selected time range
			try
			{
				var args = new RetrieveContentGroupStatisticsArgs
				{
					WorkflowInstanceIds = workflowInstanceIds,
					ReportDays = reportDays
				};
				var data = await _netStore.ExecuteAsync(args);

				var model = new ContentStatisticsViewModel
				{
					ContentDuration = data.ContentDuration,
					ExploitingDuration = data.ExploitingDuration,
					ExploitedDuration = data.ExploitedDuration,
					ExploitedPercentage = data.ExploitedPercentage,
					TaggerExploitationTime = data.TaggerExploitationTime,
					Taggers = data.Taggers,
					Tags = data.Tags,
					TaskDone = data.TaskDone,
					TaskOutstanding = data.TaskOutstanding
				};

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
				model.TotalTags = data.Tags;
				model.TotalTaggers = data.Taggers;

				// Get labels
				model.ContentDurationLabel = GetLabel(model.ContentDuration);
				model.ExploitingDurationLabel = GetLabel(model.ExploitingDuration);
				model.ExploitedDurationLabel = GetLabel(model.ExploitedDuration);
				model.ExploitedPercentageLabel = GetLabel(GetRounded(model.ExploitedPercentage));
				model.TaggerExploitationTimeLabel = GetLabel(model.TaggerExploitationTime);
				model.TodayExploitedPercentageLabel = GetLabel(GetRounded(model.TodayExploitedPercentage));
				model.TotalContentDurationLabel = GetLabel(model.TotalContentDuration);
				model.TotalExploitingDurationLabel = GetLabel(model.TotalExploitingDuration);
				model.TotalExploitedPercentageLabel = GetLabel(GetRounded(model.TotalExploitedPercentage));

				return model;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}

		public async Task<ContentProgressViewModel> GetProgressData(
			string missionInstanceId,
			double reportDays,
			int timeStep,
			string dateFormatString)
		{
			if (string.IsNullOrWhiteSpace(missionInstanceId)) throw new ArgumentException(nameof(missionInstanceId));
			if (reportDays < 0) throw new ArgumentException(nameof(missionInstanceId));
			if (timeStep < 0) throw new ArgumentException(nameof(missionInstanceId));
			if (string.IsNullOrWhiteSpace(dateFormatString)) throw new ArgumentException(nameof(dateFormatString));

			var workflowInstanceIds = await GetWorkflowInstanceIdsAsync(missionInstanceId);
			if (workflowInstanceIds == null || workflowInstanceIds.Length == 0) return new ContentProgressViewModel();

			try
			{
				var args = new RetrieveContentGroupProgressArgs
				{
					WorkflowInstanceIds = workflowInstanceIds,
					ReportDays = reportDays,
					TimeStep = timeStep
				};
				var data = await _netStore.ExecuteAsync(args);

				var model = new ContentProgressViewModel
				{
					ExploitedDuration = GetModel(data?.ExploitedDuration, dateFormatString),
					TotalDuration = GetModel(data?.TotalDuration, dateFormatString),
					TasksPerformed = GetModel(data?.TasksPerformed, dateFormatString),
					TasksOutstanding = GetModel(data?.TasksOutstanding, dateFormatString),
					TasksCompletedPerPeriod = GetModel(data?.TasksCompletedPerPeriod, dateFormatString),
					CompletionPercent = GetModel(data?.CompletionPercent, dateFormatString),
					Sessions = GetModel(data?.Sessions, dateFormatString),
					NewTaggers = GetModel(data?.NewTaggers, dateFormatString),
					ExploitationSaturation = GetModel(data?.ExploitationSaturation, dateFormatString)
				};

				// Set max and min values for better UX
				model.CompletionPercentMinValue = 0;
				model.CompletionPercentMaxValue = 100;

				model.SessionsMinValue = 0;
				if (data?.Sessions != null)
				{
					var value = GetMaxValue(data.Sessions.Select(it => it.Value));
					if (value > 0) model.SessionsMaxValue = value;
				}

				model.NewTaggersMinValue = 0;
				if (data?.NewTaggers != null)
				{
					var value = GetMaxValue(data.NewTaggers.Select(it => it.Value));
					if (value > 0) model.NewTaggersMaxValue = value;
				}

				model.ExploitationSaturationMinValue = 0;
				if (data?.ExploitationSaturation != null)
				{
					var value = GetMaxValue(data.ExploitationSaturation.Select(it => it.Value));
					if (value > 0) model.ExploitationSaturationMaxValue = value;
				}

				return model;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}

		public async Task<IEnumerable<SelectListItem>> GetMissionInstanceOptionsAsync(
			string selectedOption = null)
		{
			var options = new List<SelectListItem>
			{
				new SelectListItem { Text = "All Instances", Value = "0" },
				new SelectListItem { Text = "Active Instances", Value = "1"}
			};

			var args = new FindHyperDocumentsArgs(true);
			args.SetDocumentType(typeof(HyperMissionInstance));
			args.DescriptorConditions.AddCondition("MissionId", Request.MissionId);

			var documents = await _netStore.ExecuteAsync(args);

			options.AddRange(GetMissionInstanceOptions(documents));

			var option = options.FirstOrDefault(it => it.Value == selectedOption);
			if (option != null) option.Selected = true;

			return options;
		}

		private static List<SelectListItem> GetMissionInstanceOptions(
			HyperDocument[] documents)
		{
			if (documents == null) throw new ArgumentException(nameof(documents));

			var options = new List<SelectListItem>();

			foreach (var document in documents)
			{
				var instance = document.GetPayload<HyperMissionInstance>();
				var missionConfiguration = instance.MissionConfiguration;

				var option = new SelectListItem
				{
					Value = document.Id.Id,
					Text = GetMissionInstanceOptionText(missionConfiguration.Name, instance)
				};

				options.Add(option);
			}

			return options;
		}

		private static string GetMissionInstanceOptionText(
			string missionName,
			HyperMissionInstance instance)
		{
			if (string.IsNullOrWhiteSpace(missionName)) throw new ArgumentException(nameof(missionName));
			if (instance == null) throw new ArgumentException(nameof(instance));

			var text = missionName;

			if (instance.StopAtUTC.HasValue) text += $" ({instance.RunAtUTC:d} - {instance.StopAtUTC.Value:d})";
			else text += $" ({instance.RunAtUTC:d})";

			return text;
		}

		private async Task<string[]> GetWorkflowInstanceIdsAsync(
			string missionInstanceOption)
		{
			if (string.IsNullOrWhiteSpace(missionInstanceOption)) throw new ArgumentException(nameof(missionInstanceOption));

			if (missionInstanceOption == "0") return await GetWorkflowInstanceIdsAsync(false);
			if (missionInstanceOption == "1") return await GetWorkflowInstanceIdsAsync(true);

			return await GetWorkflowInstanceIds(missionInstanceOption);
		}

		private async Task<string[]> GetWorkflowInstanceIdsAsync(
			bool activeOnly)
		{
			var args = new FindHyperDocumentsArgs(true);
			args.SetDocumentType(typeof(HyperWorkflowInstance));
			args.DescriptorConditions.AddCondition("MissionId", Request.MissionId);

			var instances = await FindHyperDocumentsArgs.FindAsync<HyperWorkflowInstance>(_netStore, args);
			if (!activeOnly) return instances.Select(it => it.Id).ToArray();

			var statsArgs = new RetrieveHyperWorkflowsStatusesArgs { MissionIds = new[] { Request.MissionId } };
			var statuses = await _netStore.ExecuteAsync(statsArgs);
			if (statuses == null || !statuses.Any()) return new string[] { };

			var data = new List<HyperWorkflowInstance>();

			foreach (var status in statuses)
			{
				var activeInstance = instances.FirstOrDefault(it => it.Id == status.WorkflowInstanceId);
				if (activeInstance == null) continue;

				data.Add(activeInstance);
			}

			return data.Select(it => it.Id).ToArray();
		}

		private async Task<string[]> GetWorkflowInstanceIds(
			string missionInstanceId)
		{
			var args = new FindHyperDocumentsArgs(true);
			args.SetDocumentType(typeof(HyperWorkflowInstance));
			args.DescriptorConditions.AddCondition("MissionInstanceId", missionInstanceId);

			var instances = await FindHyperDocumentsArgs.FindAsync<HyperWorkflowInstance>(_netStore, args);

			return instances.Select(it => it.Id).ToArray();
		}

		private static List<KeyValueModel> GetModel(
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
					Value = GetRounded(item.Value.TotalHours)
				});
			}

			return model;
		}

		private static List<KeyValueModel> GetModel(
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

		private static List<KeyValueModel> GetModel(
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
					Value = GetRounded(item.Value)
				});
			}

			return model;
		}

		private static List<KeyValueModel> GetModel(
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

		private static long GetMaxValue(
			IEnumerable<long> values)
		{
			if (values == null) throw new ArgumentException(nameof(values));

			var max1 = values.Max();
			var max2 = max1 * 100 / 66;

			if (max1 == max2) max2 += max1;

			return max2;
		}

		private static double GetMaxValue(
			IEnumerable<double> values)
		{
			if (values == null) throw new ArgumentException(nameof(values));

			var max = values.Max();

			return max * 100 / 66;
		}


		private static double GetRounded(
			double value)
		{
			return Math.Round(value, RoundingFactor, RoundingMethod);
		}

		private static string GetLabel(
			double value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		private static string GetLabel(
			TimeSpan value)
		{
			return value.ToString(TimeSpanFormatString);
		}
	}
}
