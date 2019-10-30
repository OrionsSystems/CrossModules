using System.Collections.Generic;

using Orions.SDK.Utilities;

namespace Orions.Systems.CrossModules.Timeline.Models
{
	public class TimelineViewModel: HyperStoreModel
	{
		public Dictionary<string, AssetTagGroup> Groups;
		public List<string> TagAssetIds;
		public List<DynamicHyperTagViewModel> Tags;
		public string Json;

		public TimelineViewModel()
		{
			TagAssetIds = new List<string>();
			Groups = new Dictionary<string, AssetTagGroup>();
			Tags = new List<DynamicHyperTagViewModel>();
			Json = "";
		}
	}
}
