using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Orions.CrossModules.Common;
using Orions.XPlatform;

namespace Orions.Systems.CrossModules.Timeline
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

			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

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
			//load env specific configuration
			XPlatform.Configuration.Settings.Load(AppDomain.CurrentDomain.BaseDirectory);
			var xplatformSettings = new XPlatformSettings()
			{
				CloudAuthentication = XPlatform.Configuration.Settings.CloudAuthentication,
				ServiceBusConnectionString = XPlatform.Configuration.Settings.ServiceBusConnectionString
			};

			// init XPlatform assembly
			XPlatform.Settings.Init(xplatformSettings);

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				//app.UseHsts();
			}

			//app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseSession();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
						name: "playlist-track.m3u8",
						pattern: "playlist/hls/{connection}/{assetId}/{trackId}/track.m3u8",
						defaults: new { area = "Workbench", controller = "Playlist", action = "Hls", namespaces = new[] { "Orions.Workbench.Web.Controllers" } });

				endpoints.MapControllerRoute(
					name: "playlist-asset.m3u8",
					pattern: "playlist/hls/{connection}/{assetId}/asset.m3u8",
					defaults: new { area = "Workbench", controller = "Playlist", action = "Hls", namespaces = new[] { "Orions.Workbench.Web.Controllers" } });

				endpoints.MapControllerRoute(
					name: "playlist-track.index",
					pattern: "playlist/track/hls/{connection}/{assetId}/{trackId}/{index}",
					defaults: new { area = "Workbench", controller = "Playlist", action = "Track", namespaces = new[] { "Orions.Workbench.Web.Controllers" } });

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
