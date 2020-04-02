using EmbeddedBlazorContent;

using MatBlazor;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Orions.Systems.CrossModules.Common;
using Orions.Systems.CrossModules.Components;
using Orions.Systems.CrossModules.Sandbox.Data;
using Syncfusion.EJ2.Blazor;
using System;
using System.Linq;
using System.Net.Http;

namespace Orions.Systems.CrossModules.Sandbox
{
	public class ModuleStartup : CrossModuleStartup
	{
		public ModuleStartup(IConfiguration configuration)
			: base(configuration)
		{
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);

			services.AddSingleton<DataContext>();

			services.AddRazorPages();
			services.AddServerSideBlazor();


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
		public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{

			base.Configure(app, env);

			//Register Syncfusion license
			var syncfusionLicense = Configuration.GetValue<string>("SyncfusionLicense");
			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);

			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});

			// Workaround for https://github.com/aspnet/AspNetCore/issues/13470
			//app.Use((context, next) =>
			//{
			//	context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = null;
			//	return next.Invoke();
			//});

			app.UseEmbeddedBlazorContent(typeof(BaseOrionsComponent).Assembly);

			app.UseEmbeddedBlazorContent(typeof(BaseMatComponent).Assembly);
		}
	}
}
