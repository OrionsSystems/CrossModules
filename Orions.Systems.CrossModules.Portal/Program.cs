using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossModules.Portal
{
	public class Program
	{
		NetStore _netStore = null;

		// Fix
		public static NetStore NetStore { get; set; }

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			 Host.CreateDefaultBuilder(args)
				  .ConfigureWebHostDefaults(webBuilder =>
				  {
					  webBuilder.UseStartup<Startup>();
				  });
	}
}
