using Orions.Cloud.Common.Data.System;

namespace Orions.Systems.CrossModules.Timeline
{
	public class TimelineSettings
	{
		public static string ServerUri { get; private set; }

		public static HyperNodeRecord NodeInfo { get; private set; }

		public static void Init(string serverUri, HyperNodeRecord nodeInfo)
		{
			ServerUri = serverUri;
			NodeInfo = nodeInfo;
		}
	}
}
