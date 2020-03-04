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