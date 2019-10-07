using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EmbeddedBlazorContent;
using MatBlazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orions.CrossModules.Blazor;
using Orions.Systems.CrossModules.Common;

namespace Orions.Systems.CrossModules.MetadataReview
{
	public class Startup : CrossModuleStartup
	{
		public Startup(IConfiguration configuration)
			: base(configuration)
		{
		}

		public override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);

			services.AddRazorPages();
			services.AddServerSideBlazor();

			services.AddMatToaster(config =>
			{
				config.Position = MatToastPosition.TopRight;
				config.PreventDuplicates = false;
				config.NewestOnTop = true;
				config.ShowCloseButton = true;
			});

			//more code may be present here
			// TODO Add this back once we get latest Telerik.UI.for.Blazor 
			//services.AddTelerikBlazor();

			// Server Side Blazor doesn't register HttpClient by default - https://github.com/Suchiman/BlazorDualMode
			if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
			{
				// Setup HttpClient for server side in a client side compatible fashion
				services.AddScoped<HttpClient>(s =>
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

		public override void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseRouting();
			
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");				
			});
						

			app.UseEmbeddedBlazorContent(typeof(MatBlazor.BaseMatAccordion).Assembly);

			//Orions.CrossModules.Blazor.Common
			app.UseEmbeddedBlazorContent(typeof(BaseOrionsComponent).Assembly);			
		}
	}
}
