using Orions.Common;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public class IconMapping : UnifiedBlob
	{
		public StringComparisonMode StringCompareMode { get; set; } = StringComparisonMode.Equals;

		public IconMappingEntry[] Mappings { get; set; }

		public IconMapping()
		{
		}

		public HyperDocumentId? TryMap(string value) 
		{
			if (this.Mappings == null || this.Mappings.Length == 0)
				return null;

			if (this.StringCompareMode == StringComparisonMode.Equals)
				return this.Mappings.FirstOrDefault(it => it.Name == value)?.Icon;

			return this.Mappings.FirstOrDefault(it => it.Name.Contains(value))?.Icon;
		}
	}
}
