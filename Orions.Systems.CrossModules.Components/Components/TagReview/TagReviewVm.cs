using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Components.Helpers;

namespace Orions.Systems.CrossModules.Components
{
	public class TagReviewVm : BlazorVm, ITagReviewContext
	{
		private int _smallestPageSize;
		private HyperMetadataSet _metadataSet;
		private MasksHeatmapRenderer _renderer;

		public bool ShowFragmentAndSlice { get; set; } = true;

		public bool ExtractMode { get; set; } = true;

		public string FabricServiceId { get; set; } = "";

		public HyperDocumentId MetadataSetId { get; private set; }

		public IHyperArgsSink HyperStore { get; set; }

		public ViewModelProperty<List<HyperTag>> HyperTags { get; set; } = new ViewModelProperty<List<HyperTag>>();

		public int DashApiPort { get; set; }

		public ViewModelProperty<int> PageNumber { get; set; } = new ViewModelProperty<int>(1);
		public ViewModelProperty<int> PageSize { get; set; } = new ViewModelProperty<int>(8);
		public ViewModelProperty<int> TotalPages { get; set; } = new ViewModelProperty<int>();
		public ViewModelProperty<bool> MetadataSetLoadFailed { get; set; } = new ViewModelProperty<bool>(false);

		public int ColumnsNumber { get; set; } = 4;

		public int InitialRowsNumber { get; set; } = 2;

		public ViewModelProperty<bool> IsVmShowingHeatmapProp { get; set; } = new ViewModelProperty<bool>(false);
		public ViewModelProperty<string> HeatmapImgProp { get; set; } = new ViewModelProperty<string>();
		public MasksHeatmapRenderer.HeatmapSettings HeatmapSettings { get; set; } = new MasksHeatmapRenderer.HeatmapSettings();

		public ViewModelProperty<TagReviewFilterState> FilterState { get; set; } = new ViewModelProperty<TagReviewFilterState>(new TagReviewFilterState());

		public DateRangeSlider HeatmanRangeSlider { get; set; }
		public ViewModelProperty<string> StatusLabel { get; set; } = new ViewModelProperty<string>();
		public ViewModelProperty<string> PercentageStatusLabel { get; set; } = new ViewModelProperty<string>();

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

		public bool PlaybackRunning { get; set; } = false;
		public _HeatmapPlaybackSettings HeatmapPlaybackSettings { get; set; }
		public HeatmapStepCache HeatmapPlaybackCache { get; set; }
		public Func<DateTime, DateTime, Task> OnMetadatasetEdgeDatesUpdated;
		public Func<HeatmapStepCache, Task> OnHeatmapCacheUpdated;

		public bool PlaybackCacheIsBeingUpdated { get; set; } = false;

		public TagReviewVm()
		{
		}

		public async Task Initialize(IHyperArgsSink store, string metadataSetId, int smallestPageSize)
		{
			this.HyperStore = store;

			if (metadataSetId == null)
			{
				MetadataSetLoadFailed.Value = true;
				return;
			}

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

				await InitializeMetadatasetEdgeDates();
			}
			else
			{
				MetadataSetLoadFailed.Value = true;
			}
		}

		private async Task InitializeMetadatasetEdgeDates()
		{
			FilterState.Value.HeatMapMinDate.Value = this.FilterState.Value.MetadataSetMinDate;
			FilterState.Value.HeatMapMaxDate.Value = this.FilterState.Value.MetadataSetMaxDate;

			if (_metadataSet != null)
			{
				var findArgs = new FindHyperDocumentsArgs(typeof(HyperTag));
				var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(HyperStore, _metadataSet);
				findArgs.DescriptorConditions.AddCondition(conditions.Result);

				findArgs.Limit = 1;
				findArgs.OrderByFields = new OrderByField[]
				{
						new OrderByField()
						{
							Ascending = true,
							DescriptorField = true,
							FieldName = "Elements.UniversalTime"
						}
				};

				var results = await HyperStore.ExecuteAsync(findArgs);

				if (results?.Length > 0 == false)
					return;

				var earliestTag = results[0].GetPayload<HyperTag>();
				var earliestDate = earliestTag.GetUniversalDateTimeFromElements();

				var lastTagFindArgs = new FindHyperDocumentsArgs(typeof(HyperTag));
				lastTagFindArgs.DescriptorConditions.AddCondition(conditions.Result);
				lastTagFindArgs.OrderByFields = new OrderByField[]
				{
						new OrderByField()
						{
							Ascending = false,
							DescriptorField = true,
							FieldName = "Elements.UniversalTime"
						}
				};
				lastTagFindArgs.Limit = 1;
				var latestTag = (await HyperStore.ExecuteAsync(lastTagFindArgs))[0].GetPayload<HyperTag>();
				var latestDate = latestTag.GetUniversalDateTimeFromElements();

				this.FilterState.Value.MetadataSetMinDate.Value = earliestDate.Value;
				if (this.FilterState.Value.FilterMinDate.Value == null)
				{
					this.FilterState.Value.HeatMapMinDate.Value = earliestDate.Value;
				}
				this.FilterState.Value.MetadataSetMaxDate.Value = latestDate.Value;
				if (FilterState.Value.FilterMaxDate.Value == null)
				{
					this.FilterState.Value.HeatMapMaxDate.Value = latestDate.Value;
				}

				this.OnMetadatasetEdgeDatesUpdated?.Invoke(earliestDate.Value, latestDate.Value);
			}
		}

		private async Task<int> GetDashApiPort()
		{
			var streamingPort = 8585;
			var retrieveConfigurationArgs = new RetrieveConfigurationArgs();

			var result = await HyperStore.ExecuteAsync(retrieveConfigurationArgs);

			foreach (var item in result)
			{
				if (item.ComponentConfigType == typeof(StandardsBasedNetStoreServerConfig))
				{
					var config = new StandardsBasedNetStoreServerConfig();
					JsonHelper.Populate(item.Json, config);
					if (config.HttpsPort.HasValue)
						streamingPort = config.HttpsPort.Value;
				}
			}

			return streamingPort;
		}

		public async Task FilterTags(DateTime? startDate, DateTime? endDate, string[] filterLabels)
		{
			FitlerMetadataSet(startDate, endDate, filterLabels);
			FilterState.Value.FilterMinDate.Value = startDate;
			FilterState.Value.FilterMaxDate.Value = endDate;
			FilterState.Value.HeatMapMinDate.Value = startDate ?? FilterState.Value.MetadataSetMinDate;
			FilterState.Value.HeatMapMaxDate.Value = endDate ?? FilterState.Value.MetadataSetMaxDate;

			PageNumber.Value = 1;

			await LoadHyperTags();
			await LoadTotalPages();
		}

		public void ShowHeatmap()
		{
			IsVmShowingHeatmapProp.Value = true;

			RunHeatmapGenerationForCurrentDateFilter();
		}

		public void RunHeatmapGenerationForCurrentDateFilter()
		{
			this.HeatmapImgProp.Value = null;
			this.StatusLabel.Value = "Initializing heatmap renderer...";
			this.PercentageStatusLabel.Value = "";

			if (_renderer != null)
			{
				CancelCurrrentHeatmapGeneration();
			}
			_renderer = new MasksHeatmapRenderer(this.HyperStore, this._metadataSet, HeatmapSettings);

			_renderer.ImageProp.PropertyChanged += HetmapRendererImageProp_PropertyChanged;
			_renderer.StatusProp.PropertyChanged += HetmapRendererStatusProp_PropertyChanged;
			_renderer.PertcantageLabel.PropertyChanged += HeatmapRendererPercentageProp_PropertyChanged;

			Task.Run(() => _renderer.RunGenerationAsync(this.FilterState.Value.HeatMapMinDate.Value, this.FilterState.Value.HeatMapMaxDate.Value));
		}

		private void HeatmapRendererPercentageProp_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.PercentageStatusLabel.Value = _renderer.PertcantageLabel.Value;
		}

		private void HetmapRendererStatusProp_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"HetmapRendererStatusProp_PropertyChanged: {_renderer.StatusProp.Value}");

			this.StatusLabel.Value = _renderer.StatusProp.Value;
		}

		private void HetmapRendererImageProp_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			HeatmapImgProp.Value = $"data:image/jpg;base64, {Convert.ToBase64String(_renderer.ImageProp.Value)}";
		}

		public void CancelCurrrentHeatmapGeneration()
		{
			_renderer.ImageProp.PropertyChanged -= HetmapRendererImageProp_PropertyChanged;
			_renderer.StatusProp.PropertyChanged -= HetmapRendererStatusProp_PropertyChanged;
			_renderer.PertcantageLabel.PropertyChanged -= HeatmapRendererPercentageProp_PropertyChanged;
			_renderer.CancelGeneration();
		}

		public void CloseHeatmap()
		{
			CancelCurrrentHeatmapGeneration();
			StopHeatmapPlayback();

			IsVmShowingHeatmapProp.Value = false;
			HeatmapImgProp.Value = null;
		}

		public void HeatmapDateRangeChanged(DateTime[] newRange)
		{
			this.FilterState.Value.HeatMapMinDate.Value = newRange[0];
			this.FilterState.Value.HeatMapMaxDate.Value = newRange[1];
			RunHeatmapGenerationForCurrentDateFilter();
		}

		private HeatmapCacheHelper _heatmapCacheHelper;
		public async Task UpdateHeatmapPlaybackCache()
		{
			this.CleanStatusMessages();
			this.PlaybackCacheIsBeingUpdated = true;

			this.CancelCurrrentHeatmapGeneration();
			this.StatusLabel.Value = "Updating cache..";
			_heatmapCacheHelper = new HeatmapCacheHelper(HyperStore, HeatmapSettings);
			_heatmapCacheHelper.GenerationProgress += (completionPercent) =>
			{
				this.PercentageStatusLabel.Value = $"{completionPercent.ToString("0.0")}%";
			};
			var cache = await _heatmapCacheHelper.CreateCache(this.FilterState.Value.HeatmapEdgeMinDate.Value, this.FilterState.Value.HeatmapEdgeMaxDate.Value, _metadataSet,
				this.HeatmapPlaybackSettings != null ? this.HeatmapPlaybackSettings.StepPeriod : TimeSpan.FromHours(1));

			if (!_heatmapCacheHelper.GenerationWasCanceled)
			{
				this.OnHeatmapCacheUpdated?.Invoke(cache);

				this.HeatmapPlaybackCache = cache;
				this.StatusLabel.Value = "Cache updated";
			}
			else
			{
				this.StatusLabel.Value = "Cache generation canceled";
			}


			this._heatmapCacheHelper = null;
			this.PlaybackCacheIsBeingUpdated = false;
		}

		public void CancelCacheUpdate()
		{
			_heatmapCacheHelper.CancelCacheGeneration();
			PlaybackCacheIsBeingUpdated = false;
		}

		private CancellationTokenSource _heatmapPlaybackCts;
		public async Task RunHeatmapPlayback()
		{
			if (HeatmapPlaybackCache == null)
			{
				return;
			}

			this.CancelCurrrentHeatmapGeneration();
			this.CleanStatusMessages();

			using (_heatmapPlaybackCts = new CancellationTokenSource())
			{
				PlaybackRunning = true;

				foreach (var step in this.HeatmapPlaybackCache.Steps)
				{
					this.FilterState.Value.HeatMapMinDate = step.From;
					this.FilterState.Value.HeatMapMaxDate = step.To;
					this.HeatmapImgProp.Value = step.ImageData != null ? $"data:image/jpg;base64, {Convert.ToBase64String(step.ImageData)}" : null;

					try
					{
						await Task.Delay(HeatmapPlaybackSettings != null ? (int)HeatmapPlaybackSettings.StepPlayDuration.TotalMilliseconds : 3000, _heatmapPlaybackCts.Token);
					}
					catch (TaskCanceledException)
					{
						break;
					}
				}

				PlaybackRunning = false;
			}
			_heatmapPlaybackCts = null;
		}

		public void StopHeatmapPlayback()
		{
			_heatmapPlaybackCts?.Cancel();
			PlaybackRunning = false;
		}

		private void CleanStatusMessages()
		{
			this.StatusLabel.Value = "";
			this.PercentageStatusLabel.Value = "";
		}

		private void FitlerMetadataSet(DateTime? startDate, DateTime? endDate, string[] filterLabels)
		{
			if (this._metadataSet == null)
				return;

			if (startDate.HasValue)
			{
				this._metadataSet.FromDate = startDate.Value;
			}

			if (endDate.HasValue)
			{
				this._metadataSet.ToDate = endDate.Value;
			}

			if (filterLabels != null)
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
		}

		public async Task LoadTotalPages()
		{
			var countArgs = new CountHyperDocumentsArgs(typeof(HyperTag));

			var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(HyperStore, this._metadataSet);
			countArgs.DescriptorConditions.AddCondition(conditions.Result);

			var totalTags = await CountHyperDocumentsArgs.CountAsync<HyperTag>(this.HyperStore, countArgs);

			TotalPages.Value = (int)(totalTags % PageSize == 0 ? totalTags / PageSize : totalTags / PageSize + 1);

			RaiseNotify(nameof(TotalPages));
		}

		public async Task LoadHyperTags()
		{
			if (this.HyperStore == null)
				return;

			TagsAreBeingLoaded = true;

			var findArgs = new FindHyperDocumentsArgs(typeof(HyperTag));

			var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(HyperStore, _metadataSet);
			findArgs.DescriptorConditions.AddCondition(conditions.Result);
			findArgs.Skip = PageSize * (PageNumber - 1);
			findArgs.Limit = PageSize;

			var docs = await HyperStore.ExecuteAsync(findArgs);

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
		#region Inner classes
		public class TagReviewFilterState
		{
			public ViewModelProperty<DateTime?> MetadataSetMinDate { get; set; } = new ViewModelProperty<DateTime?>(null);
			public ViewModelProperty<DateTime?> MetadataSetMaxDate { get; set; } = new ViewModelProperty<DateTime?>(null);
			public ViewModelProperty<DateTime?> FilterMinDate { get; set; } = new ViewModelProperty<DateTime?>(null);
			public ViewModelProperty<DateTime?> FilterMaxDate { get; set; } = new ViewModelProperty<DateTime?>(null);
			public DateTime? HeatmapEdgeMinDate
			{
				get
				{
					return this.FilterMinDate.Value ?? this.MetadataSetMinDate.Value;
				}
			}
			public DateTime? HeatmapEdgeMaxDate
			{
				get
				{
					return this.FilterMaxDate.Value ?? this.MetadataSetMaxDate.Value;
				}
			}
			public ViewModelProperty<DateTime?> HeatMapMinDate { get; set; } = new ViewModelProperty<DateTime?>(null);
			public ViewModelProperty<DateTime?> HeatMapMaxDate { get; set; } = new ViewModelProperty<DateTime?>(null);
		}

		public class _HeatmapPlaybackSettings
		{
			public TimeSpan StepPlayDuration { get; set; }
			public TimeSpan StepPeriod { get; set; }
		}
		#endregion
	}
}
