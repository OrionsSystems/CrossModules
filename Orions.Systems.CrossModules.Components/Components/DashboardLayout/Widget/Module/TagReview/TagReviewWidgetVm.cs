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
		public TagReviewWidgetVm()
        {
		}


		public ViewModelProperty<TagReviewVm> TagReviewVm { get; set; } = new ViewModelProperty<TagReviewVm>(new TagReviewVm());

		public override async Task HandleFiltersChangedAsync()
		{
			var filter = this.DashboardVm.GetFilterGroup(Widget.FilterGroup);

			await this.TagReviewVm.Value. FilterTags(filter);
		}
	}
}
