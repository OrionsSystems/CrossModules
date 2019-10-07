using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orions.Systems.CrossModules.Common;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class ModuleStartup : CrossModuleStartup
	{
		public ModuleStartup(IConfiguration configuration) 
			: base(configuration)
		{
		}

		public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			base.Configure(app, env);

			app.UseMvc(routes =>
			{
				// Default
				routes.MapRoute(
					name: "default_area",
					template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

				// Default
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

			app.UseKendo(env);
		}

		public override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);

			services.AddMvc()
				//AddJsonOptions(options => options.JsonSerializerOptions.ContractResolver = new DefaultContractResolver())
				.AddSessionStateTempDataProvider()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			// Add Kendo UI services to the services container
			services.AddKendo();
		}
	}
}
