using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.NodeStats
{
	public class NodeDataManager : NotifyPropertyChanged
	{
		NetStore _netStore;

		public string Name { get; set; }

		public HyperUserLoginAttempt[] Logins { get; set; }

		string _status = "Loading...";
		public string Status => _status;

		public RetrieveConfigurationArgs.ConfigurationData[] Configs { get; set; }

		long _missionsCount = 0;
		long _workflowsCount = 0;
		long _assetsCount = 0;

		public string[] BadgeSuccesses
		{
			get
			{
				return new string[] { };
			}
		}

		HotSwapListLite<string> _notifications = new HotSwapListLite<string>();

		public string[] BadgeNotifications
		{
			get
			{
				return _notifications.ToArray();
			}
		}

		HotSwapListLite<string> _errors = new HotSwapListLite<string>();

		public string[] BadgeError
		{
			get
			{
				return _errors.ToArray();
			}
		}

		HotSwapListLite<string> _warnings = new HotSwapListLite<string>();

		public string[] BadgeWarnings
		{
			get
			{
				return _warnings.ToArray();
			}
		}

		RetrieveHardwareStatusArgs.ResultData _fullStatus = null;

		public RetrieveHardwareStatusArgs.ResultData FullStatus => _fullStatus;

		public int DetailedDrivesInfoCount => FullStatus?.DetailedDrivesInfo?.Length ?? 0;

		public WMIHelper.DetailDriveInfo[] DrivesInfo => _fullStatus?.DetailedDrivesInfo ?? new WMIHelper.DetailDriveInfo[] { };

		public string OnlineStatus
		{
			get
			{
				string online = $"Online {_fullStatus?.OSInfo?.OnlineFor?.Days:00}.{_fullStatus?.OSInfo?.OnlineFor?.Hours:00}:{_fullStatus?.OSInfo?.OnlineFor?.Minutes:00}";
				return online;
			}
		}

		public string SDKStatus
		{
			get
			{
				var authFull = _netStore?.DefaultAuthenticationInfo as HyperAuthenticationInfo;
				return "SDK " + authFull?.ServerSDKVersion;
			}
		}

		public string MissionStatus
		{
			get
			{
				return "Missions " + _missionsCount;
			}
		}

		public string WorkflowStatus
		{
			get
			{
				return "Workflows " + _workflowsCount;
			}
		}

		public string AssetStatus
		{
			get
			{
				return "Assets " + _assetsCount;
			}
		}

		object _syncRoot = new object();

		Queue<double> _cpuHistory = new Queue<double>();
		Queue<double> _ramHistory = new Queue<double>();
		Queue<double> _activityHistory = new Queue<double>();

		public object[] CPUHistory
		{
			get
			{
				lock(_syncRoot)
					return _cpuHistory.Cast<object>().ToArray();
			}
		}

		public object[] RAMHistory
		{
			get
			{
				lock (_syncRoot)
					return _ramHistory.Cast<object>().ToArray();
			}
		}

		public object[] ActivityHistory
		{
			get
			{
				lock (_syncRoot)
					return _activityHistory.Cast<object>().ToArray();
			}
		}


		/// <summary>
		/// Ctor
		/// </summary>
		public NodeDataManager()
		{
		}

		public async Task InitAsync(string name, string connectionUrl)
		{
			try
			{
				this.Name = name;
				_netStore = await NetStore.ConnectAsyncThrows(connectionUrl);
			}
			catch (Exception ex)
			{
			}
		}

		void SetDiskNotification(bool value)
		{
			string raw = "DISK";
			if (value)
			{
				if (_errors.Any(it => it == raw) == false)
					_errors.Add(raw);
			}
			else
			{
				_errors.Remove(raw);
			}
		}

		void SetCPUNotification(bool value)
		{
			string raw = "CPU";
			if (value)
			{
				if (_warnings.Any(it => it == raw) == false)
					_warnings.Add(raw);
			}
			else
			{
				_warnings.Remove(raw);
			}
		}

		void SetRAMNotification(bool value)
		{
			string raw = "RAM";
			if (value)
			{
				if (_warnings.Any(it => it == raw) == false)
					_warnings.Add(raw);
			}
			else
			{
				_notifications.Remove(raw);
			}
		}

		int _refreshCycle = 0;
		int _refreshCount = 25;

		public async Task RunUpdatesAsync()
		{
			var store = _netStore;
			if (store == null)
				return;

			Interlocked.Increment(ref _refreshCycle);

			{
				try
				{
					RetrieveConfigurationArgs.ConfigurationData[] configs = await store.ExecuteAsync(new RetrieveConfigurationArgs());
					Configs = configs;
				}
				catch (Exception ex)
				{
				}
			}

			{
				try
				{
					var loginsArgs = new FindHyperDocumentsArgs(typeof(HyperUserLoginAttempt));
					loginsArgs.Limit = 20;
					loginsArgs.DescriptorConditions.AddCondition(nameof(HyperUserLoginAttempt.DateTimeUTC), DateTime.UtcNow - TimeSpan.FromMinutes(120), ScopeCondition.Comparators.GreaterThanOrEqual);

					var docs = await store.ExecuteAsync(loginsArgs);

					Logins = docs?.Select(it => it.GetPayload<HyperUserLoginAttempt>()).Where(it => it != null).ToArray() ?? new HyperUserLoginAttempt[] { };
					Logins = Logins?.GroupBy(x => x.UserName).Select(x => x.FirstOrDefault()).ToArray(); // Distincts by UserName
				}
				catch (Exception ex)
				{
				}
			}

			{
				try
				{
					var countArgs = new CountHyperDocumentsArgs(typeof(HyperMission));
					_missionsCount = await store.ExecuteAsync(countArgs);
				}
				catch (Exception ex)
				{
				}
			}

			{
				try
				{
					var countArgs = new CountHyperDocumentsArgs(typeof(HyperWorkflow));
					_workflowsCount = await store.ExecuteAsync(countArgs);
				}
				catch (Exception ex)
				{
				}
			}

			{
				try
				{
					var logsArgs = new CountLogsArgs() { Age = TimeSpan.FromSeconds(10) };
					var logsCount = await store.ExecuteAsync(logsArgs);

					if (_refreshCycle % 2 == 0)
					{
						lock (_syncRoot)
						{
							_activityHistory.Enqueue(logsCount);
							while (_activityHistory.Count > _refreshCount)
								_activityHistory.Dequeue();
						}
					}
				}
				catch (Exception)
				{
				}
			}

			{
				try
				{
					var countArgs = new CountHyperDocumentsArgs(typeof(HyperAsset));
					_assetsCount = await store.ExecuteAsync(countArgs);
				}
				catch (Exception ex)
				{
				}
			}

			{
				try
				{
					var fullStatus = await store.ExecuteAsync(new RetrieveHardwareStatusArgs() { RetrieveDetailedDrivesInfo = true, RetrieveOSInfo = true, RetrieveProcessorsInfo = true });
					_fullStatus = fullStatus;

					SetRAMNotification(false);
					SetCPUNotification(false);
					SetDiskNotification(false);


					if (fullStatus != null)
					{
						_status = $"CPU {fullStatus.ProcessorsInfo?.Average(it => it.PercentProcessorTime):#0.0}% RAM {fullStatus.OSInfo?.PercentageUsedRAM:##.0}% ";

						var cpuTime = fullStatus.ProcessorsInfo.Average(it => it.PercentProcessorTime);

						if (cpuTime.HasValue && _refreshCycle % 2 == 0)
						{
							lock (_syncRoot)
							{
								_cpuHistory.Enqueue(cpuTime.Value);
								while (_cpuHistory.Count > _refreshCount)
									_cpuHistory.Dequeue();
							}
						}

						var ram = fullStatus.OSInfo?.PercentageUsedRAM;

						if (ram.HasValue && _refreshCycle % 2 == 0)
						{
							lock (_syncRoot)
							{
								_ramHistory.Enqueue(ram.Value);
								while (_ramHistory.Count > _refreshCount)
									_ramHistory.Dequeue();
							}
						}

						SetCPUNotification(cpuTime > 50);
						SetRAMNotification(ram > 50);

						SetDiskNotification(fullStatus.DetailedDrivesInfo.Any(it => it.Progress > 0.9)); // 80% disk drive.

						Notify(nameof(Status));
					}
				}
				catch (Exception ex)
				{
				}
			}

			Notify(nameof(Configs));
			Notify(nameof(FullStatus));

		}
	}
}
