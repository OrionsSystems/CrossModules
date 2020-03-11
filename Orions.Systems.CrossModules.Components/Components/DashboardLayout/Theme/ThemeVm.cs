using Microsoft.AspNetCore.Components;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class ThemeVm : BlazorVm
	{

		public List<StyleTheme> DataList = new List<StyleTheme>();

		public StyleTheme SelectedData { get; private set; }

		public EventCallback<StyleTheme> OnOpenTheme { get; set; }

		public bool IsShowProperty { get; private set; }

		public bool IsLoadedDataResult { get; private set; }

		public bool ShowConfirmDeleteTheme { get; set; }

		public PropertyGridVm PropertyGridVm { get; set; } = new PropertyGridVm();

		IHyperArgsSink _hyperStore = null;
		public IHyperArgsSink HyperStore
		{
			get { return _hyperStore; }
			set { _hyperStore = value; }
		}

		public ThemeVm()
		{
		}

		public async Task InitAsync()
		{
			if (this.HyperStore != null)
				await LoadThemeList();
		}

		public async Task InitTheme(string themeId) 
		{
			if (this.HyperStore != null)
				SelectedData =  await GetTheme(themeId);
		}

		public async Task<StyleTheme> GetTheme(string themeId)
		{
			if (string.IsNullOrWhiteSpace(themeId))
				return null;

			var documentId = HyperDocumentId.Create<StyleTheme>(themeId);
			var args = new RetrieveHyperDocumentArgs(documentId);
			var doc = await HyperStore.ExecuteAsync(args);

			if (args.ExecutionResult.IsNotSuccess)
				return null;

			var theme = doc?.GetPayload<StyleTheme>();
			return theme;
		}

		public Task<object> LoadTheme()
		{
			return Task.FromResult<object>(SelectedData);
		}

		public async Task CreateTheme() 
		{
			var newTheme = new StyleTheme();
			//var data = await HyperStore.RetrieveAsync<StyleTheme>(newTheme.Id);
			
			// TODO show in theme view
			SelectedData = newTheme;
			await SaveChangesAsync();

			await LoadThemeList();
		}

		public void ShowDeleteConfirmationDialog(StyleTheme data)
		{
			ShowConfirmDeleteTheme = true;
			SelectedData = data;
		}

		public async Task OnDeleteTheme()
		{
			if (SelectedData == null) return;
			await DeleteTheme(SelectedData);

			ShowConfirmDeleteTheme = false;
		}

		public async Task DeleteTheme(StyleTheme data)
		{
			var args = new DeleteHyperDocumentArgs(HyperDocumentId.Create<StyleTheme>(data.Id));
			var isDeleteSuccessful = await HyperStore.ExecuteAsync(args);

			if (isDeleteSuccessful)
			{
				DataList.RemoveAll(it => it.Id == data.Id);
			}
		}

		public async Task<Response> SaveChangesAsync()
		{
			var doc = new HyperDocument(SelectedData);

			var args = new StoreHyperDocumentArgs(doc);
			await HyperStore.ExecuteAsync(args);

			var res =  args.ExecutionResult.AsResponse();

			return res;
		}

		public async Task LoadThemeList()
		{
			// We are only going to pull the basic info on the Dashboards, as they are becoming large objects.
			var datas = await HyperStore.FindAllAsync<StyleTheme>(null, 0, 500, true);

			DataList = datas.ToList();

			IsLoadedDataResult = true;

			this.RaiseNotify(nameof(DataList));
		}

		public async Task OpenThemeAsync(StyleTheme data)
		{
			await OnOpenTheme.InvokeAsync(data);
		}

		public async Task OpenPropertyGridAsync(StyleTheme data)
		{
			data = await HyperStore.RetrieveAsync<StyleTheme>(data.Id); // Pull the full data.

			SelectedData = data;
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

			await SaveChangesAsync();

			await LoadThemeList();
		}
	}
}
