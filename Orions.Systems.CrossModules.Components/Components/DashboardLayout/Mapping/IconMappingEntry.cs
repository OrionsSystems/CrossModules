using Orions.Common;
using Orions.Node.Common;
using Orions.Infrastructure.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class IconMappingEntry : UnifiedBlob
	{
		[HelpText("The Id of the Icon", HelpTextAttribute.Priorities.Mandatory)]
		[HyperDocumentId.DocumentType(typeof(UniResource))]
		public HyperDocumentId? Icon { get; set; }

		public string Name { get; set; }

		public IconMappingEntry()
		{
		}
	}
}
