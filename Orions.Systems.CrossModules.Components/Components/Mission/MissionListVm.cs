using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class MissionListVm : BlazorVm
	{
		public bool IsLoadedData { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public PropertyGridVm PropertyGridVm { get; set; } = new PropertyGridVm();

		public EventCallback<HyperMission> OnManage { get; set; }

		public bool IsShowProperty { get; set; }

		public List<MissionItemVm> Items { get; set; } = new List<MissionItemVm>();

		public HyperMission SelectedItem { get; set; }

		public string SearchInput { get; set; }


		public MissionListVm()
		{
		}

		public async Task Init()
		{
			if (HyperStore == null) return;

			await Populate();

			IsLoadedData = true;
		}

		private async Task Populate()
		{
			Items.Clear();
			IsLoadedData = false;

			var findArgs = new FindHyperDocumentsArgs();
			findArgs.SetDocumentType(typeof(HyperMission));
			findArgs.DescriptorConditions.Mode = AndOr.Or;
			findArgs.DescriptorConditions.AddCondition(Assist.GetPropertyName((HyperMission i) => i.Archived), false);
			findArgs.DescriptorConditions.AddCondition(Assist.GetPropertyName((HyperMission i) => i.Archived), false, Comparers.DoesNotExist);
			var docs = await HyperStore.ExecuteAsync(findArgs);

			if (docs == null || !docs.Any())
				return;

			foreach (var doc in docs)
			{
				var data = doc.GetPayload<HyperMission>();
				if (data == null)
				{
					await OnToastMessage.InvokeAsync(new ToastMessage($"Failed to load data from document: {data.Id}", ToastMessageType.Error));
					continue;
				}

				var model = new MissionItemVm(data, HyperStore);
				await model.UpdateStatus();
				model.OnToastMessage = OnToastMessage;

				Items.Add(model);
			}

			IsLoadedData = true;
		}

		public async Task OnSearchBtnClick(MouseEventArgs e)
		{
			//TODO
		}

		public async Task CreateNew()
		{
			//TODO
		}

		public async Task ManageAsync(MissionItemVm item)
		{
			await OnManage.InvokeAsync(item.Mission);
		}

		public void ShowPropertyGrid(MissionItemVm item)
		{
			SelectedItem = item.Mission;
			IsShowProperty = true;
		}

		public Task<object> LoadPropertyGrid()
		{
			return Task.FromResult<object>(SelectedItem);
		}

		public void OnCancelProperty()
		{
			PropertyGridVm.CleanSourceCache();
			IsShowProperty = false;
		}
	}
}
