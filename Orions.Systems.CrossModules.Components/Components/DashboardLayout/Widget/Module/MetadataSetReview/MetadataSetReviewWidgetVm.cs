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

		public UniFilterData Filter { get; private set; }

		public override async Task HandleFiltersChangedAsync()
		{
			var filter = this.DashboardVm.DynamicFilter;

			this.Filter = filter;

			RaiseNotify("Filter");
		}
	}
}
