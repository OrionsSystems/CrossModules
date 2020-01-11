using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Orions.Infrastructure.HyperMedia;
using System.Threading.Tasks;
using Orions.Common;

namespace Orions.Systems.CrossModules.Portal
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			try
			{
				ReflectionHelper.Instance.GatherExtraNativeAssemblies();
				CreateHostBuilder(args).Build().Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to start: " + Assist.PrintException(ex));
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			 Host.CreateDefaultBuilder(args)
				  .ConfigureWebHostDefaults(webBuilder =>
				  {
					  webBuilder.UseStartup<Startup>();
				  });
	}
}
