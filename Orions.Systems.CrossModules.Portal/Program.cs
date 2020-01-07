using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Orions.Infrastructure.HyperMedia;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			try
			{
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
