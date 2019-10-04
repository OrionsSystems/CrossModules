using Orions.CrossModules.Common;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossModules.NodeStats
{
	public class Program
	{
		static AppHostHelper<Startup> _helper;

		public static CrossModuleInstanceHost CrossModuleInstanceHost => _helper?.CrossModuleInstanceHost;

		/// <summary>
		/// Main entry point for the application.
		/// </summary>
		public static void Main(string[] args)
		{
			_helper = new AppHostHelper<Startup>();

			_helper.OnReady += OnReadyHandler;

			_helper.StartWithArgs(args);
		}

		private static void OnReadyHandler(AppHostHelper helper)
		{
			// Feed in the data for the Node connections to the local Nodes Manager.
			// var z = NodesManager.Instance.InitAsync(helper?.AppliedConfig?.NodeConnections);
		}
	}
}
