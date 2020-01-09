using System;
using System.Linq;
using System.Net.Http;
using EmbeddedBlazorContent;
using MatBlazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orions.Systems.CrossModules.Components;
using Syncfusion.EJ2.Blazor;

namespace Orions.Systems.CrossModules.Portal
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();

			services.AddMatToaster(config =>
			{
				config.Position = MatToastPosition.TopRight;
				config.PreventDuplicates = true;
				config.NewestOnTop = true;
				config.ShowCloseButton = true;
				config.MaximumOpacity = 95;
				config.VisibleStateDuration = 3000;
			});

			services.AddTelerikBlazor();

			services.AddSyncfusionBlazor();

			services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

			// Server Side Blazor doesn't register HttpClient by default - https://github.com/Suchiman/BlazorDualMode
			if (services.All(x => x.ServiceType != typeof(HttpClient)))
			{
				// Setup HttpClient for server side in a client side compatible fashion
				services.AddScoped(s =>
				{
					// Creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
					var uriHelper = s.GetRequiredService<NavigationManager>();
					return new HttpClient
					{
						BaseAddress = new Uri(uriHelper.BaseUri)
					};
				});
			}
		}


		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			//Register Syncfusion license
			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTk0MDU3QDMxMzcyZTM0MmUzMGJIcnNicmc0ek1LU0dqNHQ1bERLTzUybFk1YllGb25wVnlEVk9WZ3JDbUU9");

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});

			app.UseEmbeddedBlazorContent(typeof(BaseOrionsComponent).Assembly);
			app.UseEmbeddedBlazorContent(typeof(BaseMatComponent).Assembly);
		}
	}
}
