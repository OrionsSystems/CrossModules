using Microsoft.AspNetCore.Components;

using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardListVm : BlazorVm
	{
		public bool IsShowRenameDashboardModal { get; set; }

		public bool IsShowModalImportProject { get; set; }

		public bool IsShowProperty { get; private set; }

		public bool IsLoadedDataResult { get; private set; }

		/// <summary>
		/// The full dashboard data must be assigned here.
		/// </summary>
		public DashboardData SelectedDashboard { get; private set; }

		/// <summary>
		/// Limited (name + id only) dashboard data is assigned here.
		/// </summary>
		public List<DashboardData> DataList = new List<DashboardData>();

		public PropertyGridVm PropertyGridVm { get; set; }

		public EventCallback<DashboardData> OnSelectDesign { get; set; }

		public EventCallback<DashboardData> OnSelectView { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public DashboardListVm()
		{
		}

		public async Task InitAsync()
		{
			if (this.HyperStore != null)
				await LoadDashboarList();
		}

		public Task<object> LoadDashboard()
		{
			return Task.FromResult<object>(SelectedDashboard);
		}

		public async Task LoadDashboarList()
		{
			// We are only going to pull the basic info on the Dashboards, as they are becoming large objects.
			var datas = await HyperStore.FindAllAsync<DashboardData>(null, 0, 500, true);

			DataList = datas.ToList();

			IsLoadedDataResult = true;

			this.RaiseNotify(nameof(DataList));
			//StateHasChanged();
		}

		public async Task SelectDashboardAsync(DashboardData data, bool showView = false, bool isNew = false)
		{
			if (isNew == false)
				data = await HyperStore.RetrieveAsync<DashboardData>(data.Id); // Pull the full data.

			if (showView)
			{
				await OnSelectView.InvokeAsync(data);
			}
			else
			{
				await OnSelectDesign.InvokeAsync(data);
			}
		}

		public async Task SaveChanges()
		{
			var doc = new HyperDocument(SelectedDashboard);

			var args = new StoreHyperDocumentArgs(doc);
			var res = await HyperStore.ExecuteAsync(args);

			IsShowRenameDashboardModal = false;

			await LoadDashboarList();
		}

		public async Task DeleteDashboard(DashboardData data)
		{
			var args = new DeleteHyperDocumentArgs(HyperDocumentId.Create<DashboardData>(data.Id));
			var isDeleteSuccessful = await HyperStore.ExecuteAsync(args);

			if (isDeleteSuccessful)
			{
				DataList.RemoveAll(it => it.Id == data.Id);
			}
		}

		public async Task ImportProject(byte[] bytes, bool isNew = true)
		{
			var json = Encoding.Default.GetString(bytes);

			if (string.IsNullOrWhiteSpace(json))
				return;

			var res = JsonHelper.Deserialize<DashboardData>(json);

			if (isNew)
				res.Id = IdHelper.GenerateId();

			//update and save
			if (res == null) return;

			SelectedDashboard = res;

			await SaveChanges();
		}

		public async Task EditNameAsync(DashboardData data)
		{
			data = await HyperStore.RetrieveAsync<DashboardData>(data.Id); // Pull the full data.

			SelectedDashboard = data;
			IsShowRenameDashboardModal = true;
		}

		public async Task OpenPropertyGridAsync(DashboardData data)
		{
			data = await HyperStore.RetrieveAsync<DashboardData>(data.Id); // Pull the full data.

			SelectedDashboard = data;
			IsShowProperty = true;
		}

		public void OnCancelProperty()
		{
			PropertyGridVm.CleanSourceCache();
			IsShowProperty = false;
		}

		public async Task OnOkProperty()
		{
			PropertyGridVm.CleanSourceCache();
			IsShowProperty = false;

			await SaveChanges();
		}
	}
}
