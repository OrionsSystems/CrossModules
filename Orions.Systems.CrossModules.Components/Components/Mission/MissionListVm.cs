using Microsoft.AspNetCore.Components.Web;

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

		public bool IsShowProperty { get; set; }

		public List<HyperMission> Items { get; set; } = new List<HyperMission>();

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
			var docs = await HyperStore.ExecuteAsync(findArgs);

			if (docs == null || !docs.Any())
				return;

			foreach (var workflow in docs)
			{
				var data = workflow.GetPayload<HyperMission>();
				if (data == null)
				{
					Console.WriteLine($"Failed to load data from document: {data.Id}");
					continue;
				}

				Items.Add(data);
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

		public void ShowPropertyGrid(HyperMission item)
		{
			SelectedItem = item;
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
