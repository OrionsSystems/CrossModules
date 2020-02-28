using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Components.Helpers;

namespace Orions.Systems.CrossModules.Components
{
	public class TagReviewVm : BlazorVm
	{
		private int _smallestPageSize;
		private HyperMetadataSet _metadataSet;
		private MasksHeatmapRenderer _renderer;

		public TagReviewVm()
		{
		}

		public HyperDocumentId MetadataSetId { get; private set; }
		public IHyperArgsSink Store { get; set; }
		public ViewModelProperty<List<HyperTag>> HyperTags = new ViewModelProperty<List<HyperTag>>();
		public int DashApiPort { get; set; }
		//public UniFilterData Filter { get; private set; }
		public ViewModelProperty<int> PageNumber { get; set; } = new ViewModelProperty<int>(1);
		public ViewModelProperty<int> PageSize { get; set; } = new ViewModelProperty<int>(8);
		public ViewModelProperty<int> TotalPages { get; set; } = new ViewModelProperty<int>();
		public ViewModelProperty<bool> MetadataSetLoadFailed { get; set; } = new ViewModelProperty<bool>(false);
		public int ColumnsNumber { get; set; } = 4;
		public int InitialRowsNumber { get; set; } = 2;
		public ViewModelProperty<bool> IsVmShowingHeatmapProp { get; set; } = new ViewModelProperty<bool>(false);
		public ViewModelProperty<string> HeatmapImgProp { get; set; } = new ViewModelProperty<string>();
		public MasksHeatmapRenderer.HeatmapSettings HeatmapSettings { get; set; } = new MasksHeatmapRenderer.HeatmapSettings();

		public List<int> PageSizeOptions
		{
			get
			{
				return new List<int>()
				{
					_smallestPageSize,
					_smallestPageSize * 2,
					_smallestPageSize * 3,
					_smallestPageSize * 6,
				};
			}
		}
		public bool PlayerOpened { get; set; } = false;
		public bool TagsAreBeingLoaded { get; set; } = false;
		public string PlayerUri { get; set; }
		public string PlayerId { get; set; }

		public async Task Initialize(IHyperArgsSink store, string metadataSetId, int smallestPageSize)
		{
			this.Store = store;

			MetadataSetId = new HyperDocumentId(metadataSetId, typeof(HyperMetadataSet));

			var metadataSetFilter = await store.ExecuteAsync(new RetrieveHyperDocumentArgs(MetadataSetId));

			if (metadataSetFilter != null)
			{
				this._smallestPageSize = smallestPageSize;

				PageSize = smallestPageSize;

				this._metadataSet = metadataSetFilter.GetPayload<HyperMetadataSet>();
				this.DashApiPort = await GetDashApiPort();

				await LoadTotalPages();
				await LoadHyperTags();
			}
			else
			{
				MetadataSetLoadFailed.Value = true;
			}
		}

		private void ImageProp_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			HeatmapImgProp.Value = $"data:image/jpg;base64, {Convert.ToBase64String(_renderer.ImageProp.Value)}";
		}

		private async Task<int> GetDashApiPort()
		{
			var streamingPort = 8585;
			var retrieveConfigurationArgs = new RetrieveConfigurationArgs();

			var result = await Store.ExecuteAsync(retrieveConfigurationArgs);

			foreach (var item in result)
			{
				if (item.ComponentConfigType == typeof(StandardsBasedNetStoreServerConfig))
				{
					var config = new StandardsBasedNetStoreServerConfig();
					JsonHelper.Populate(item.Json, config);
					if (config.HttpPort.HasValue)
						streamingPort = config.HttpPort.Value;
				}
			}

			return streamingPort;
		}

		public async Task FilterTags(DateTime? startDate, DateTime? endDate, string[] filterLabels)
		{
			//this.Filter = filter;
			FitlerMetadataSet(startDate, endDate, filterLabels);

			PageNumber.Value = 1;

			await LoadHyperTags();
			await LoadTotalPages();
		}

		public void ShowHeatmap()
		{
			_renderer = new MasksHeatmapRenderer(this.Store, this._metadataSet, HeatmapSettings);
			_renderer.ImageProp.PropertyChanged += ImageProp_PropertyChanged;
			IsVmShowingHeatmapProp.Value = true;
			Task.Run(_renderer.RunGenerationAsync);
		}

		public void CloseHeatmap()
		{
			_renderer.ImageProp.PropertyChanged -= ImageProp_PropertyChanged;
			_renderer.CancelGeneration();
			IsVmShowingHeatmapProp.Value = false;
			HeatmapImgProp.Value = null;
		}

		private void FitlerMetadataSet(DateTime? startDate, DateTime? endDate, string[] filterLabels)
		{
			if (startDate.HasValue)
			{
				this._metadataSet.FromDate = startDate.Value;
			}

			if (endDate.HasValue)
			{
				this._metadataSet.ToDate = endDate.Value;
			}

			if(filterLabels != null)
			{
				this._metadataSet.TextFilters = filterLabels.Select(it =>
				{
					var elements = it.Split(":");
					if (elements.Count() == 0)
						return String.Empty;
					else
						return elements[elements.Count() - 1];
				}).ToArray();
			}
			//if (filter is MultiFilterData multiFilter)
			//{
			//	foreach (var subFilter in multiFilter.Elements)
			//	{
			//		SetMetadataSetFilter(subFilter);
			//	}
			//}
			//else
			//{
			//	SetMetadataSetFilter(filter);
			//}
		}

		//private void SetMetadataSetFilter(IUniFilterData filter)
		//{
		//	if (filter is DateTimeFilterData dateTimefilter)
		//	{
		//		this._metadataSet.FromDate = dateTimefilter.StartTime;
		//		this._metadataSet.ToDate = dateTimefilter.EndTime;
		//	}

		//	if (filter is TextFilterData textFilterData)
		//	{
		//		this._metadataSet.TextFilters = textFilterData.LabelsArray?.Select(it =>
		//		{
		//			var elements = it.Split(":");
		//			if (elements.Count() == 0)
		//				return String.Empty;
		//			else
		//				return elements[elements.Count() - 1];
		//		}).ToArray();
		//	}
		//}

		public async Task LoadTotalPages()
		{
			var countArgs = new CountHyperDocumentsArgs(typeof(HyperTag));

			var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(Store, this._metadataSet);
			countArgs.DescriptorConditions.AddCondition(conditions.Result);

			var totalTags = await CountHyperDocumentsArgs.CountAsync<HyperTag>(this.Store, countArgs);

			TotalPages.Value = (int)(totalTags % PageSize == 0 ? totalTags / PageSize : totalTags / PageSize + 1);

			RaiseNotify(nameof(TotalPages));
		}

		public async Task LoadHyperTags()
		{
			TagsAreBeingLoaded = true;

			var findArgs = new FindHyperDocumentsArgs(typeof(HyperTag));

			var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(Store, _metadataSet);
			findArgs.DescriptorConditions.AddCondition(conditions.Result);
			findArgs.Skip = PageSize * (PageNumber - 1);
			findArgs.Limit = PageSize;

			var docs = await Store.ExecuteAsync(findArgs);

			var hyperTags = new List<HyperTag>();
			foreach (var doc in docs)
			{
				hyperTags.Add(doc.GetPayload<HyperTag>());
			}

			HyperTags.Value = hyperTags;

			TagsAreBeingLoaded = false;

			RaiseNotify(nameof(HyperTags));
		}

		public async Task ChangePageSize(int pageSize)
		{
			CalculatePageNumberForNewPageSize(pageSize);
			PageSize.Value = pageSize;

			await LoadHyperTags();
			await LoadTotalPages();
		}

		private void CalculatePageNumberForNewPageSize(int newPageSize)
		{
			var currentFirstItemIndex = this.PageSize * (this.PageNumber - 1) + 1;
			var pageNumber = currentFirstItemIndex / newPageSize + 1;

			PageNumber.Value = pageNumber;
		}

		public async Task ChangePage(string pageNumberStr)
		{
			int pageNumber = 1;
			int.TryParse(pageNumberStr, out pageNumber);

			if (pageNumber < 1)
			{
				pageNumber = 1;
			}
			if (pageNumber > TotalPages.Value)
			{
				pageNumber = TotalPages.Value;
			}

			if (pageNumber != PageNumber.Value)
			{
				PageNumber.Value = pageNumber;
				HyperTags.Value = new List<HyperTag>();
				await LoadHyperTags();
			}
		}
	}
}
