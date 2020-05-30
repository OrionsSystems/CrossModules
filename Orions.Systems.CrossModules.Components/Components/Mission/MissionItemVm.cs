
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class MissionItemVm : BlazorVm
	{
		public IHyperArgsSink HyperStore { get; set; }

		public HyperMission Mission { get; private set; }

		public HyperMissionInstance MissionInstance { get; set; }
		public HyperMissionInstanceStatus InstanceStatus { get; set; }

		public AdvancedObservableCollection<HyperMissionPhase> Phases { get; set; } = new AdvancedObservableCollection<HyperMissionPhase>();
		public ViewModelProperty<HyperMissionPhase> SelectedPhaseProp { get; set; } = new ViewModelProperty<HyperMissionPhase>();

		public BlazorCommand AddPhaseCommand { get; set; } = new BlazorCommand();
		public BlazorCommand RemovePhaseCommand { get; set; } = new BlazorCommand();

		public BlazorCommand ClearCommand { get; set; } = new BlazorCommand();
		public BlazorCommand ClearCommandWarning { get; set; } = new BlazorCommand();

		public BlazorCommand LoadCommand { get; set; } = new BlazorCommand();
		public BlazorCommand UnloadCommand { get; set; } = new BlazorCommand();

		public BlazorCommand LoadInstanceCommand { get; set; } = new BlazorCommand();
		public BlazorCommand LoadNewInstanceCommand { get; set; } = new BlazorCommand();

		public BlazorCommand ViewWorkflowCommand { get; set; } = new BlazorCommand();
		public BlazorCommand ViewWorkflowInstanceCommand { get; set; } = new BlazorCommand();

		public string StatusText { get; set; }

		public MissionItemVm(HyperMission mission, IHyperArgsSink store)
		{
			Mission = mission;
			HyperStore = store;

			ClearCommandWarning.AsyncDelegate = OnClearCommandWarningAsync;
			ClearCommand.AsyncDelegate = OnClearCommandAsync;
			LoadCommand.Delegate = OnLoadCommand;
			UnloadCommand.AsyncDelegate = OnUnloadCommandAsync;
		}

		public async Task UpdateStatus()
		{
			UpdateStatusColors();

			await UpdateModelStatusAsync();
		}

		public async Task OnUnloadCommandAsync(DefaultCommand command, object parameter)
		{
			string missionId = Mission.Id;
			var store = HyperStore;
			if (store == null || missionId == null)
				return;

			var args = new UnloadHyperMissionArgs() { MissionId = missionId };
			var result = await store.ExecuteAsync(args);

			await UpdateModelStatusAsync();
			//await UpdateMissionInstancesAsync();

			await OnToastMessage.InvokeAsync(new ToastMessage($"Unload mission", ToastMessageType.Info));
		}

		public async Task OnClearCommandWarningAsync(DefaultCommand command, object parameter)
		{

			await OnToastMessage.InvokeAsync(new ToastMessage($"This operation will delete all mission-generated data!", ToastMessageType.Info));

			//TODO open dialog for confirmation!
		}

		public async Task OnClearCommandAsync(DefaultCommand command, object parameter)
		{
			//TODO
			//if (MessageBox.Show("***WARNING*** This operation will delete all mission-generated data!! Are you sure?", "WARNING", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
			//	return;

			if (HyperStore == null)
				return;

			if (this.InstanceStatus != null && this.InstanceStatus.State != HyperMissionInstanceStatus.States.Unloaded)
			{
				//TODO
				await OnToastMessage.InvokeAsync(new ToastMessage($"This mission is currently running. You must stop the mission before clearing data.", ToastMessageType.Warning));
				return;
			}

			string missionId = Mission.Id;

			string missionInstanceId = MissionInstance?.Id;

			var clearArgs = new ClearHyperMissionDataArgs() { MissionId = missionId, MissionInstanceId = missionInstanceId };
			int deleted = await this.HyperStore.ExecuteAsync(clearArgs);

			if (clearArgs.ExecutionResult.IsSuccess)
			{
				await OnToastMessage.InvokeAsync(new ToastMessage($"UOperation succeeded, removed a total of {clearArgs.Result} documents", ToastMessageType.Info));
			}
			else
			{
				await OnToastMessage.InvokeAsync(new ToastMessage($"Operation failed: {clearArgs.ExecutionResult.Message}", ToastMessageType.Info));
			}
		}

		public async void OnLoadCommand(DefaultCommand command, object parameter)
		{
			await this.LoadModelOnNodeAsync(HyperStore, MissionInstance);

			//UpdatePhasesUI();
			//await UpdateMissionInstancesAsync();

			await OnToastMessage.InvokeAsync(new ToastMessage($"Load mission", ToastMessageType.Info));
		}

		private async Task<string> LoadModelOnNodeAsync(IHyperArgsSink store, HyperMissionInstance instance = null, bool allowAutoMissionResume = true)
		{
			if (this.Mission == null)
				return string.Empty;

			this.ResetModelStatus();

			if (store == null)
			{
				await OnToastMessage.InvokeAsync(new ToastMessage($"No Node selected, loading model failed.", ToastMessageType.Info));
			}

			var args = new LoadHyperMissionArgs() { AllowMissionAutoResume = allowAutoMissionResume };
			if (instance == null)
				args.MissionConfigurationId = Mission.Id;
			else
				args.MissionInstance = instance;

			var missionStatus = await store.ExecuteAsync(args);

			if (missionStatus == null)
			{
				if (args.ExecutionResult.IsNotSuccess)
				{
					await OnToastMessage.InvokeAsync(new ToastMessage(args.ExecutionResult.Message, ToastMessageType.Error));
				}
				else
				{
					await OnToastMessage.InvokeAsync(new ToastMessage($"Failed to load model instance.", ToastMessageType.Info));
				}
			}

			if (this != null)
				await this.UpdateModelStatusAsync();

			return "Model loading, status: " + missionStatus;
		}

		private void ResetModelStatus()
		{
			InstanceStatus = null;
		}

		void UpdateStatusColors()
		{
			//TODO
			//if (this.IsModified)
			//	this.StatusForeground = new SolidColorBrush(Colors.Red);
			//else
			//	this.StatusForeground = new SolidColorBrush(Colors.Black);

			var status = this.InstanceStatus;

			if (status != null && status.State == HyperMissionInstanceStatus.States.Loaded)
			{
				//StatusBackground = new SolidColorBrush(Colors.Green);
				StatusText = "Loaded";
			}
			else if (status != null && status.State == HyperMissionInstanceStatus.States.Loading)
			{
				//StatusBackground = new SolidColorBrush(Colors.Yellow);
				StatusText = "Loading";
			}
			else
			{
				//StatusBackground = new SolidColorBrush(Colors.LightGray);
				StatusText = "Unloaded";
			}
		}

		bool _callPending = false;
		private async Task UpdateModelStatusAsync()
		{
			string missionId = Mission.Id;
			if (HyperStore == null || missionId == null || _callPending)
				return;

			var args = new GetHyperMissionStatusArgs() { MissionId = missionId };
			_callPending = true;
			try
			{
				InstanceStatus = await HyperStore.ExecuteAsync(args);
			}
			finally
			{
				_callPending = false;
			}
		}

		public async Task<bool> ArchiveHyperMission()
		{
			var netStore = HyperStore;

			if (netStore == null || this.Mission == null || String.IsNullOrEmpty(this.Mission.Id))
				return (false);

			var args = new ArchiveHyperMissionArgs() { MissionId = this.Mission.Id };
			await netStore.ExecuteAsync(args);

			if (args.ExecutionResult.IsSuccess)
			{
				await OnToastMessage.InvokeAsync(new ToastMessage($"Operation succeeded. Mission was archived", ToastMessageType.Info));
				return (true);
			}

			await OnToastMessage.InvokeAsync(new ToastMessage($"Operation failed: {args.ExecutionResult.Message}", ToastMessageType.Info));
			return (false);
		}

		void OnRemovePhase(DefaultCommand command, object parameter)
		{
			var phaseModel = this.SelectedPhaseProp.Value;
			HyperMission mission = this.Mission;
			if (mission == null || phaseModel == null)
				return;

			mission.Phases.Remove(phaseModel);
		}
	}
}
