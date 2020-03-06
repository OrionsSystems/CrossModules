using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(TagReviewWidget))]
	public class TagReviewWidgetVm : WidgetVm<TagReviewWidget>
	{
		public ViewModelProperty<TagReviewVm> TagReviewVm { get; set; } = new ViewModelProperty<TagReviewVm>(new TagReviewVm());

		public TagReviewWidgetVm()
		{
		}

		public void Initialize()
		{
			var store = this.HyperStore;

			this.TagReviewVm.Value.ColumnsNumber = this.Widget.ColumnsNumber;
			this.TagReviewVm.Value.InitialRowsNumber = this.Widget.InitialRowsNumber;
			this.TagReviewVm.Value.HeatmapSettings = this.Widget.HeatmapSettings;
			this.TagReviewVm.Value.FilterState.Value.MetadataSetMinDate.Value = this.Widget.Cache.MetadataSetMinDate;
			this.TagReviewVm.Value.FilterState.Value.MetadataSetMaxDate.Value = this.Widget.Cache.MetadataSetMaxDate;

			this.TagReviewVm.Value.OnMetadatasetEdgeDatesUpdated = async delegate (DateTime from, DateTime to)
			{
				this.Widget.Cache.MetadataSetMinDate = from;
				this.Widget.Cache.MetadataSetMaxDate = to;
				this.DashboardVm.SaveChangesAsync();
			};

			this.TagReviewVm.Value.Initialize(store, Widget.MetadataSetId?.Id, Widget.ColumnsNumber * Widget.InitialRowsNumber);
		}

		protected override void OnWidgetSet(IDashboardWidget widget)
		{
			base.OnWidgetSet(widget);

			TagReviewVm.Value.ShowFragmentAndSlice = this.Widget.ShowFragmentAndSlice;
			TagReviewVm.Value.ExtractMode = this.Widget.ExtractMode;
			TagReviewVm.Value.FabricServiceId = this.Widget.FabricService;
		}

		public override async Task HandleFiltersChangedAsync()
		{
			if (this.HyperStore == null || this.TagReviewVm.Value.HyperStore == null)
				return;

			//throw new NotImplementedException();

			var filter = this.DashboardVm.ObtainFilterGroup(Widget.FilterGroup);
			await this.TagReviewVm.Value.FilterTags(filter.StartTime, filter.EndTime, filter.FilterLabels);
		}
	}
}