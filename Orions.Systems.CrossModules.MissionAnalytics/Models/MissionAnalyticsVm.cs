using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Blazor;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class MissionAnalyticsVm : BlazorVm
	{
		private const int RoundingFactor = 2;
		private const MidpointRounding RoundingMethod = MidpointRounding.AwayFromZero;
		private const string TimeSpanFormatString = "c";

		private NetStore _netStore;

		private CrossModuleVisualizationRequest _request;

		public ViewModelProperty<FilterViewModel> FilterVmProp { get; set; }

		public ViewModelProperty<ContentStatisticsViewModel> StatsVmProp { get; set; }

		public ViewModelProperty<ContentProgressVm> ProgressVmProp { get; set; }

		public BlazorCommand LoadCommand { get; set; }

		public MissionAnalyticsVm()
		{
			FilterVmProp = new ViewModelProperty<FilterViewModel>(new FilterViewModel());
			StatsVmProp = new ViewModelProperty<ContentStatisticsViewModel>(new ContentStatisticsViewModel());
			ProgressVmProp = new ViewModelProperty<ContentProgressVm>(new ContentProgressVm());

			LoadCommand = new BlazorCommand();
			LoadCommand.AsyncDelegate += OnLoadAsync;
		}

		public async Task InitStoreAsync(
			HyperConnectionSettings connection)
		{
			if (connection == null) throw new ArgumentException(nameof(connection));
			if (string.IsNullOrWhiteSpace(connection.ConnectionUri)) throw new ArgumentException(nameof(connection.ConnectionUri));

			_netStore = await NetStore.ConnectAsyncThrows(connection.ConnectionUri);

			_request = OwnerComponent?.GetObjectFromQueryString<CrossModuleVisualizationRequest>("request");
			if (_request == null) _request = GetDefaultRequest();

			FilterVmProp.Value.MissionInstanceOptions = await GetMissionInstanceOptionsAsync();
			FilterVmProp.Value.SelectedMissionInstance = _request.MissionInstanceId;
		}

		public async Task LoadDataAsync()
		{
			try
			{
				StatsVmProp.Value = await GetStatsVm();

				ProgressVmProp.Value = await GetProgressVm();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private async Task OnLoadAsync(DefaultCommand command, object parameter)
		{
			await LoadDataAsync();
		}

		private async Task<IEnumerable<SelectListItem>> GetMissionInstanceOptionsAsync(
			string selectedOption = null)
		{
			var options = new List<SelectListItem>
			{
				new SelectListItem { Text = "All Instances", Value = "0" },
				new SelectListItem { Text = "Active Instances", Value = "1"}
			};

			var args = new FindHyperDocumentsArgs(true);
			args.SetDocumentType(typeof(HyperMissionInstance));
			args.DescriptorConditions.AddCondition("MissionId", _request.MissionId);

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

		private CrossModuleVisualizationRequest GetDefaultRequest()
		{
			return new CrossModuleVisualizationRequest
			{
				MissionIds = new[] { "e8d88c8a-b82d-4911-9539-3080ef877653" },
				MissionInstanceIds = new[] { "0" }
			};
		}

		private async Task<ContentStatisticsViewModel> GetStatsVm()
		{
			if (string.IsNullOrWhiteSpace(FilterVmProp.Value.SelectedMissionInstance)) throw new ArgumentException(nameof(FilterVmProp.Value.SelectedMissionInstance));
			if (string.IsNullOrWhiteSpace(FilterVmProp.Value.SelectedTimeRange)) throw new ArgumentException(nameof(FilterVmProp.Value.SelectedTimeRange));

			var workflowInstanceIds = await GetWorkflowInstanceIdsAsync(FilterVmProp.Value.SelectedMissionInstance);
			if (workflowInstanceIds == null || workflowInstanceIds.Length == 0) return new ContentStatisticsViewModel();

			// Get stats for the selected time range
			try
			{
				var args = new RetrieveContentGroupStatisticsArgs
				{
					WorkflowInstanceIds = workflowInstanceIds,
					ReportDays = FilterVmProp.Value.TimeRangeValue
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

		private async Task<ContentProgressVm> GetProgressVm()
		{
			if (string.IsNullOrWhiteSpace(FilterVmProp.Value.SelectedMissionInstance)) throw new ArgumentException(nameof(FilterVmProp.Value.SelectedMissionInstance));
			if (string.IsNullOrWhiteSpace(FilterVmProp.Value.SelectedTimeRange)) throw new ArgumentException(nameof(FilterVmProp.Value.SelectedTimeRange));

			var workflowInstanceIds = await GetWorkflowInstanceIdsAsync(FilterVmProp.Value.SelectedMissionInstance);
			if (workflowInstanceIds == null || workflowInstanceIds.Length == 0) return new ContentProgressVm();

			try
			{
				var args = new RetrieveContentGroupProgressArgs
				{
					WorkflowInstanceIds = workflowInstanceIds,
					ReportDays = FilterVmProp.Value.TimeRangeValue,
					TimeStep = GetTimeStep(FilterVmProp.Value.TimeRangeValue)
				};
				var data = await _netStore.ExecuteAsync(args);

				var dateFormatString = GetDateTimeFormatString(FilterVmProp.Value.TimeRangeValue);

				var model = new ContentProgressVm
				{
					ExploitedDurationProp = { Value = GetModel(data?.ExploitedDuration, dateFormatString) },
					TotalDurationProp = { Value = GetModel(data?.TotalDuration, dateFormatString) },
					TasksPerformedProp = { Value = GetModel(data?.TasksPerformed, dateFormatString) },
					TasksOutstandingProp = { Value = GetModel(data?.TasksOutstanding, dateFormatString) },
					TasksCompletedPerPeriodProp = { Value = GetModel(data?.TasksCompletedPerPeriod, dateFormatString) },
					CompletionPercentProp = { Value = GetModel(data?.CompletionPercent, dateFormatString) },
					SessionsProp = { Value = GetModel(data?.Sessions, dateFormatString) },
					NewTaggersProp = { Value = GetModel(data?.NewTaggers, dateFormatString) },
					ExploitationSaturationProp = { Value = GetModel(data?.ExploitationSaturation, dateFormatString) },
					CompletionPercentMinValueProp = { Value = 0 },
					SessionsMinValueProp = { Value = 0 },
					NewTaggersMinValueProp = { Value = 0 },
					ExploitationSaturationMinValueProp = { Value = 0 }
				};

				if (data?.CompletionPercent != null)
				{
					var value = GetMaxValue(data.CompletionPercent.Select(it => it.Value));
					if (value > 0) model.CompletionPercentMaxValueProp.Value = value;
				}

				if (data?.Sessions != null)
				{
					var value = GetMaxValue(data.Sessions.Select(it => it.Value));
					if (value > 10) model.SessionsMaxValueProp.Value = value;
				}

				if (data?.NewTaggers != null)
				{
					var value = GetMaxValue(data.NewTaggers.Select(it => it.Value));
					if (value > 10) model.NewTaggersMaxValueProp.Value = value;
				}

				if (data?.ExploitationSaturation != null)
				{
					var value = GetMaxValue(data.ExploitationSaturation.Select(it => it.Value));
					if (value > 0) model.ExploitationSaturationMaxValueProp.Value = value;
				}

				model.CategoriesProp.Value = model.ExploitedDurationProp.Value.Select(it => it.Key).ToArray();

				return model;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
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
			args.DescriptorConditions.AddCondition("MissionId", _request.MissionId);

			var instances = await FindHyperDocumentsArgs.FindAsync<HyperWorkflowInstance>(_netStore, args);
			if (!activeOnly) return instances.Select(it => it.Id).ToArray();

			var statsArgs = new RetrieveHyperWorkflowsStatusesArgs { MissionIds = new[] { _request.MissionId } };
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

		private static int GetTimeStep(
			double timeRange)
		{
			switch (timeRange)
			{
				case TimeRange.LastHour:
					//return 2;
					return 10;
				case TimeRange.Last2Hours:
					//return 5;
					return 10;
				case TimeRange.Last3Hours:
					//return 10;
					return 15;
				case TimeRange.Last6Hours:
					//return 20;
					return 30;
				case TimeRange.Last12Hours:
					//return 25;
					return 60;
				case TimeRange.LastDay:
					//return 30;
					return 2 * 60;
				case TimeRange.Last3Days:
					//return 60;
					return 6 * 60;
				case TimeRange.LastWeek:
					// return 2 * 60
					return 24 * 60;
				case TimeRange.LastMonth:
					//return 6 * 60; 
					return 3 * 24 * 60;
				case TimeRange.Last3Months:
					//return 3 * 24 * 60; 
					return 10 * 24 * 60;
				case TimeRange.Last6Months:
					//return 3 * 24 * 60; 
					return 15 * 24 * 60;
				case TimeRange.LastYear:
					//return 7 * 24 * 60; 
					return 30 * 24 * 60;
				case TimeRange.Ever:
					//return 30 * 24 * 60;
					return 30 * 24 * 60;
				default:
					throw new NotImplementedException();
			}
		}

		private static string GetDateTimeFormatString(
			double timeRange)
		{
			switch (timeRange)
			{
				case TimeRange.LastHour:
					return "h:mm tt";
				case TimeRange.Last2Hours:
					return "h:mm tt";
				case TimeRange.Last3Hours:
					return "h:mm tt";
				case TimeRange.Last6Hours:
					return "h:mm tt";
				case TimeRange.Last12Hours:
					return "h:mm tt";
				case TimeRange.LastDay:
					return "M/d h tt";
				case TimeRange.Last3Days:
					return "M/d h tt";
				case TimeRange.LastWeek:
					return "d";
				case TimeRange.LastMonth:
					return "d";
				case TimeRange.Last3Months:
					return "d";
				case TimeRange.Last6Months:
					return "d";
				case TimeRange.LastYear:
					return "M/yyyy";
				case TimeRange.Ever:
					return "M/yyyy";
				default:
					throw new NotImplementedException();
			}

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
