using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Common;

namespace Orions.Systems.CrossModules.MetadataReview
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
		}
	}
}
