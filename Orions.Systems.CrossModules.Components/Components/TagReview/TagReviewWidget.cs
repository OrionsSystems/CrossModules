using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Components.Helpers;
using System;

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

		public bool ShowFragmentAndSlice { get; set; } = false;

		[HelpText("Only show the extracted images of the tags, not the whole image")]
		public bool ExtractMode { get; set; } = false;

		[HelpText("If we want the images in the display to be processed with a Fabric service on request, this is the name")]
		public string FabricService { get; set; }

		[UniBrowsable(false)]
		public TagReviewWidgetCache Cache { get; set; } = new TagReviewWidgetCache();

		public TagReviewWidget()
		{
			this.Label = "Tag Review";
		}
	}

	public class TagReviewWidgetCache
	{
		public DateTime? MetadataSetMinDate { get; set; }
		public DateTime? MetadataSetMaxDate { get; set; }
	}
}
