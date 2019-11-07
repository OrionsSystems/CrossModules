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

using Orions.Systems.CrossModules.Components;
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

			services.AddTelerikBlazor();

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

		public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
						

			app.UseEmbeddedBlazorContent(typeof(BaseMatAccordion).Assembly);

			app.UseEmbeddedBlazorContent(typeof(BaseOrionsComponent).Assembly);			
		}
	}
}
