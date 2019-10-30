using System;
using System.Collections.Generic;

using Orions.SDK.Utilities;

namespace Orions.Systems.CrossModules.Timeline.Models
{
	public class TagPageFilterModel: SuperFilterModel
	{
		public class PositionRange
		{
			public ulong FromSeconds { get; private set; }
			public ulong ToSeconds { get; private set; }

			public PositionRange(
				ulong fromSeconds,
				ulong toSeconds)
			{
				FromSeconds = fromSeconds;
				ToSeconds = toSeconds;
			}
		}

		public List<string> Ids { get; set; }

		public int? PageNumber { get; set; }
		public int? PageSize { get; set; }

		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }

		public string WorkflowInstanceId { get; set; }
		public string MissionInstanceId { get; set; }

		public List<string> AssetIds { get; set; }

		public bool GroupAndOrganize { get; set; }

		public string FilterValue { get; set; }

		public string RealmId { get; set; }

		public bool Children { get; set; }

		public string ParentId { get; set; }

		public PositionRange Range { get; set; }

		public TagPageFilterModel()
		{
			Ids = new List<string>();
			AssetIds = new List<string>();
		}
	}
}
