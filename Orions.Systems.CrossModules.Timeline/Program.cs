using System;
using System.Linq;

using Orions.Cloud.Common.Data.System;
using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Common;

namespace Orions.Systems.CrossModules.Timeline
{
	public class Program
	{
		static AppHostHelper<ModuleStartup> _helper;

		public static void Main(string[] args)
		{
			_helper = new AppHostHelper<ModuleStartup>();

			_helper.OnReady += OnReadyHandler;

			_helper.StartWithArgs(args);
		}

		private static void OnReadyHandler(AppHostHelper helper)
		{
			// Feed in the data for the Node connections to the local Nodes Manager.
			// var z = NodesManager.Instance.InitAsync(helper?.AppliedConfig?.NodeConnections);

			string nodeConnectionString = null;
			var serverUri = "";

			if (helper.AppliedConfig?.NodeConnections != null)
			{
				var nodeConnectionStrings = helper.AppliedConfig?.NodeConnections;

				if (nodeConnectionStrings != null && nodeConnectionStrings.Any())
				{
					HyperConnectionSettings firstConnectionString = nodeConnectionStrings.First();

					if (!string.IsNullOrEmpty(firstConnectionString.ConnectionUri)) {
						nodeConnectionString = firstConnectionString.ConnectionUri;
						serverUri = firstConnectionString.ConnectionUri;
					} 
				}
			}

			var nodeInfo = new HyperNodeRecord
			{
				Id = $"{Guid.NewGuid()}",
				ServerUri = serverUri,
				Connection = new HyperNodeConnection
				{
					ConnectionString = nodeConnectionString ?? ""
				}
			};

			TimelineSettings.Init(serverUri, nodeInfo);
		}
	}
}
