using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(MissionListWidget))]
	public class MissionListWidgetVm : WidgetVm<MissionListWidget>
	{
		private HyperDomainHelper _hyperDomainHelper;
		private HyperConnectionSettings[] _nodes;

		public ObservableCollection<HyperWorkflowStatus> Missions { get; set; } = new ViewModelProperty<ObservableCollection<HyperWorkflowStatus>>(new ObservableCollection<HyperWorkflowStatus>());

		public MissionListWidgetVm()
		{
		}

		public async Task Initialize()
		{
			await InitializeNodesList();
			await UpdateMissionCollections();
		}

		private async Task UpdateMissionCollections()
		{
			Missions.Clear();
			var workflowStatuses = new List<HyperWorkflowStatus>();

			foreach(var node in _nodes)
			{
				var retrieveWorkflowsArgs = new RetrieveHyperWorkflowsStatusesArgs();
				var statuses = await HyperStore.ExecuteAsync(retrieveWorkflowsArgs);
				var missions = new List<HyperWorkflowStatus>();
				if (statuses != null)
				{
					foreach (var status in statuses)
					{
						var isMaster = status.SlaveWorkflowStatuses != null && status.SlaveWorkflowStatuses.Any();
						var isSlave = status.ParentWorkflowInstanceId != null;

						if (!isMaster && !isSlave)
						{
							var hasTasks = status.ActiveTaskPhases != null && status.ActiveTaskPhases.Length > 0 &&
										   status.ActiveTaskPhases.Sum(x => x.PendingTasksCount) > 0;
							if (hasTasks)
							{
								missions.Add(status);
							}
						}
						else if (isSlave)
						{
							if (!status.IsCompleted)
							{
								missions.Add(status);
							}
						}
					}
				}

				missions = missions.Where(m => !string.IsNullOrEmpty(m.MissionId)).ToList();

				missions.ForEach(m => Missions.Add(m));
			}
		}

		private async Task InitializeNodesList()
		{
			_hyperDomainHelper = new HyperDomainHelper("andrei", "a9090xxx");
			await _hyperDomainHelper.LoginToDomainAsyncThrows();

			_nodes = (await _hyperDomainHelper.TryRetrieveAccessibleNodesAsync()).Result;
		}


	}
}
