using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Common;
using System.Linq;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class Program
	{
		static AppHostHelper<ModuleStartup> _helper;

		public static CrossModuleInstanceHost CrossModuleInstanceHost => _helper?.CrossModuleInstanceHost;

		public static HyperConnectionSettings HyperConnectionSettings => _helper?.AppliedConfig?.NodeConnections.FirstOrDefault();

		/// <summary>
		/// Main entry point for the application.
		/// </summary>
		public static void Main(string[] args)
		{
			_helper = new AppHostHelper<ModuleStartup>();

			_helper.StartWithArgs(args);
		}
	}
}
