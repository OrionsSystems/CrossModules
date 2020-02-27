using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Components.Helpers;

namespace Orions.Systems.CrossModules.Components
{
	[HelpText("Helps visualize the individual tags from the selected MetadataSet + runtime filtering range")]
	[Compatibility("MetadataTagReviewWidget", "MetadataSetReviewWidget")]
	public class TagReviewWidget : DashboardWidget
	{
		[HelpText("The Id of the metadata set to use", HelpTextAttribute.Priorities.Mandatory)]
		[HyperDocumentId.DocumentType(typeof(HyperMetadataSet))]
		public HyperDocumentId? MetadataSetId { get; set; }

		public MasksHeatmapRenderer.HeatmapSettings HeatmapSettings { get; set; } = new MasksHeatmapRenderer.HeatmapSettings();

		public int ColumnsNumber { get; set; } = 6;

		public int InitialRowsNumber { get; set; } = 3;

		public TagReviewWidget()
		{
			this.Label = "Tag Review";
		}
	}
}
