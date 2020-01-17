using System;
using System.Collections.Generic;
using System.Text;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Components.Components.DashboardLayout.Widget.Module.MetadataSetReview
{
   public class MetadataSetReviewWidget : DashboardWidget, IDashboardWidget
   {
      [HelpText("The Id of the metadata set to use", HelpTextAttribute.Priorities.Mandatory)]
      [HyperDocumentId.DocumentType(typeof(HyperMetadataSet))]
      public HyperDocumentId? MetadataSetId { get; set; }

      public int ColumnsNumber { get; set; } = 4;

      public MetadataSetReviewWidget()
      {
         this.Label = "Metadata Review Widget";
      }
   }
}
