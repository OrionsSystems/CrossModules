using Orions.CrossModules.Common;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class Program
	{
		static AppHostHelper<ModuleStartup> _helper;

		public static CrossModuleInstanceHost CrossModuleInstanceHost => _helper?.CrossModuleInstanceHost;

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
