using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components.Components.DashboardLayout.Widget.Module.MetadataSetReview
{
    [Config(typeof(MetadataSetReviewWidget))]
    public class MetadataSetReviewWidgetVm : WidgetVm<MetadataSetReviewWidget>
    {
		public MetadataSetReviewWidgetVm()
        {
		}


		public ViewModelProperty<MetadataReviewVm> MetadataReviewVm { get; set; } = new ViewModelProperty<MetadataReviewVm>(new MetadataReviewVm());

		public override async Task HandleFiltersChangedAsync()
		{
			var filter = this.DashboardVm.GetFilterGroup(Widget.FilterGroup);

			await this.MetadataReviewVm.Value.FilterTags(filter);
		}
	}
}
