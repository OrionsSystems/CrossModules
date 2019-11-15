using System.Linq;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Common;
using Orions.Systems.CrossModules.Sandbox.Data;

namespace Orions.Systems.CrossModules.Sandbox
{
	public class Program
	{
		static AppHostHelper<ModuleStartup> _helper;

		public static CrossModuleInstanceHost CrossModuleInstanceHost => _helper?.CrossModuleInstanceHost;

		public static HyperConnectionSettings HyperConnectionSettings => _helper?.AppliedConfig?.NodeConnections.FirstOrDefault();

		public static void Main(string[] args)
		{
			_helper = new AppHostHelper<ModuleStartup>();

			_helper.OnReady += OnReadyHandler;

			_helper.StartWithArgs(args);
		}

		private static async void OnReadyHandler(AppHostHelper helper)
		{
			var nodeConnection = helper?.AppliedConfig?.NodeConnections.FirstOrDefault();
			await DataContext.Instance.InitStoreAsync(nodeConnection);
		}
	}
}
