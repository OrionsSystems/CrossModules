using Microsoft.AspNetCore.Components.Web;

using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class MissionEditorVm : BlazorVm
	{
		public bool IsLoadedData { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public PropertyGridVm PropertyGridVm { get; set; } = new PropertyGridVm();

		public MissionItemVm Source { get; set; }

		public HyperMissionPhase SelectedPhase { get; set; }

		public string MissionId { get; set; }


		public MissionEditorVm()
		{
		}

		public async Task Init()
		{
			if (HyperStore == null)
			{
				await OnToastMessage.InvokeAsync(new ToastMessage("Missing Hyper Store", ToastMessageType.Error));
				IsLoadedData = true;
				return;
			}

			if (string.IsNullOrWhiteSpace(MissionId))
			{
				await OnToastMessage.InvokeAsync(new ToastMessage("Missing mission id", ToastMessageType.Error));
				IsLoadedData = true;
				return;
			}

			await Populate();

			IsLoadedData = true;
		}

		private async Task Populate()
		{
			IsLoadedData = false;

			if (HyperStore == null || string.IsNullOrWhiteSpace(MissionId)) return;

			var documentId = HyperDocumentId.Create<HyperMission>(MissionId);
			var argTagonomy = new RetrieveHyperDocumentArgs(documentId);
			var doc = await HyperStore.ExecuteAsync(argTagonomy);

			if (argTagonomy.ExecutionResult.IsNotSuccess)
				return;

			var data = doc?.GetPayload<HyperMission>();

			Source = new MissionItemVm(data, HyperStore);
			await Source.UpdateStatus();
			Source.OnToastMessage = OnToastMessage;

			SelectedPhase = Source.Mission?.Phases.FirstOrDefault();

			IsLoadedData = true;
		}

		public async Task OnSelectPhase(HyperMissionPhase phase)
		{
			SelectedPhase = phase;
			await LoadPropertyGrid();
		}

		public async Task OnAddPhase()
		{
			if (Source == null || Source.Mission == null)
				return;

			var phase = new HyperMissionPhase() { Name = "New Stage" };
			Source.Mission.Phases.Add(phase);
			SelectedPhase = phase;
			await LoadPropertyGrid();
		}

		public async Task OnRemovePhase(HyperMissionPhase phase)
		{
			if (Source == null || Source.Mission == null)
				return;

			Source.Mission.Phases.Remove(phase);

			SelectedPhase = Source.Mission?.Phases.FirstOrDefault();
			await LoadPropertyGrid();
		}

		public Task<object> LoadPropertyGrid()
		{
			PropertyGridVm.CleanSourceCache();
			return Task.FromResult<object>(SelectedPhase);
		}

	}
}
