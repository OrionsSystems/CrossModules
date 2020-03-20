using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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

		public bool ShowConfirmDeleteDashboard { get; set; }

		/// <summary>
		/// The full dashboard data must be assigned here.
		/// </summary>
		public DashboardData SelectedDashboard { get; private set; }

		/// <summary>
		/// Limited (name + id only) dashboard data is assigned here.
		/// </summary>
		public List<DashboardData> DataList = new List<DashboardData>();

		public PropertyGridVm PropertyGridVm { get; set; }

		public EventCallback<string> OnSelectDesign { get; set; }

		public EventCallback<string> OnSelectView { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public string SearchInput { get; set; }

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

		public Dictionary<string, List<DashboardData>> GetDataGroups()
		{
			var result = new Dictionary<string, List<DashboardData>>();
			if (DataList == null || !DataList.Any()) return result;

			return DataList.GroupBy(x => x.Group).ToDictionary(gdc => gdc.Key ?? "Default", gdc => gdc?.ToList() ?? new List<DashboardData>());
		}

		public async Task LoadDashboarList(string filter = null)
		{
			DataList.Clear();
			IsLoadedDataResult = false;

			if (string.IsNullOrWhiteSpace(filter))
			{
				// We are only going to pull the basic info on the Dashboards, as they are becoming large objects.
				var datas = await HyperStore.FindAllAsync<DashboardData>(null, 0, 500, true);

				DataList = datas.ToList();
			}
			else {

				var findDocArgs = new FindHyperDocumentsArgs(typeof(DashboardData), true);

				var regexText = $"/.*{filter}.*/i";

				var conditions = new MultiScopeCondition(AndOr.Or);
				conditions.AddCondition(nameof(DashboardData.Name), regexText, Comparers.Regex);
				conditions.AddCondition(nameof(DashboardData.Group), regexText, Comparers.Regex);
				conditions.AddCondition(nameof(DashboardData.Tag), regexText, Comparers.Regex);

				findDocArgs.DescriptorConditions = conditions;

				var documents = await HyperStore.ExecuteAsync(findDocArgs);

				DataList = documents.Select(x => x.GetPayload<DashboardData>()).ToList();

				if (!findDocArgs.ExecutionResult.IsNotSuccess || documents != null)
				{
					DataList = documents.Select(x => x.GetPayload<DashboardData>()).ToList();
				}
			}

			IsLoadedDataResult = true;

			this.RaiseNotify(nameof(DataList));
		}

		public async Task SelectDashboardAsync(DashboardData data, bool showView = false, bool isNew = false)
		{
			if (showView)
			{
				await OnSelectView.InvokeAsync(data.Id);
			}
			else
			{
				await OnSelectDesign.InvokeAsync(data.Id);
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

		public void ShowDeleteConfirmationDialog(DashboardData data) {
			ShowConfirmDeleteDashboard = true;
			SelectedDashboard = data;
		}

		public async Task OnDeleteDashboard() {
			if (SelectedDashboard == null) return;
			await DeleteDashboard(SelectedDashboard);

			ShowConfirmDeleteDashboard = false;
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

		public async Task OnSearchBtnClick(MouseEventArgs e)
		{
			if (this.HyperStore == null) return;

			await LoadDashboarList(SearchInput);
		}
	}
}
