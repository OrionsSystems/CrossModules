using Orions.CrossModules.Common;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossModules.BlazorSample
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

			_helper.StartWithArgs(args);
		}
	}
}
