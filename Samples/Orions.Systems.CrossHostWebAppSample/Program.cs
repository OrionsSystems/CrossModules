using Orions.CrossModules.Common;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossHostWebAppSample
{
	public class Program
	{
		static AppHostHelper<CrossModuleStartup> _helper;

		public static CrossModuleInstanceHost CrossModuleInstanceHost => _helper?.CrossModuleInstanceHost;

		/// <summary>
		/// Main entry point for the application.
		/// </summary>
		public static void Main(string[] args)
		{
			_helper = new AppHostHelper<CrossModuleStartup>();

			_helper.OnReady += OnReadyHandler;

			_helper.StartWithArgs(args);
		}

		private static void OnReadyHandler(AppHostHelper helper)
		{

		}
	}
}
