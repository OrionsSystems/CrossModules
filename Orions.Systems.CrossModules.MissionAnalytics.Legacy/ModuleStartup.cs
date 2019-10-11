using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Orions.Systems.CrossModules.Common;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class ModuleStartup : CrossModuleStartup
	{
		public ModuleStartup(IConfiguration configuration) 
			: base(configuration)
		{
		}

		public override void ConfigureServices(IServiceCollection services)
		{
			services.AddControllersWithViews();
			services.AddServerSideBlazor();
			services.AddRazorPages();

			base.ConfigureServices(services);

			services.AddMvc()
				.AddSessionStateTempDataProvider()
				.SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
				.AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

			services.AddDistributedMemoryCache();

			services.AddSession();

			services.AddCors(options => options.AddPolicy("AllowAllOrigins", builder => builder.AllowAnyOrigin()));

			// Add Kendo UI services to the services container
			services.AddKendo();
		}

		public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			base.Configure(app, env);

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				// Default
				endpoints.MapControllerRoute(
					name: "default_area",
					pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

				// Default
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");

				endpoints.MapRazorPages();
				endpoints.MapBlazorHub();
			});
		}
	}
}
