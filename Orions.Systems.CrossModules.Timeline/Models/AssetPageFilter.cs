using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Timeline.Models
{
	public class AssetPageFilter
	{
		public IEnumerable<string> AssetIds { get; set; }

		public int Page { get; set; }

		public int PageSize { get; set; }

		public AssetPageFilter() {
			AssetIds = new List<string>();
		}
	}
}
