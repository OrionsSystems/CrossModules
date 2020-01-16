using System;
using System.Collections.Generic;
using System.Text;
using Orions.Common;

namespace Orions.Systems.CrossModules.Components.Components.DashboardLayout.Widget.Module.MetadataSetReview
{
    public class MetadataSetReviewWidget : DashboardWidget, IDashboardWidget
    {
        public string MetadataSetId { get; set; }

		public int ColumnsNumber { get; set; } = 4;

		public MetadataSetReviewWidget()
        {
            this.Label = "Metadata Review Widget";
        }
    }
}
