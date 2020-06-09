using Blazored.LocalStorage;

using EmbeddedBlazorContent;

using MatBlazor;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orions.Common;
using Orions.Systems.CrossModules.Components;
using Orions.Systems.CrossModules.Components.Desi.Services;
using Orions.Systems.CrossModules.Portal.Domain.Desi;
using Orions.Systems.CrossModules.Portal.Providers;
using Orions.Systems.CrossModules.Portal.Services;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.Desi.Common.TagonomyExecution;

using ReactNOW.Integration;
using Orions.SDK;

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

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();

			services.AddTelerikBlazor();

			services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

			services.AddSyncfusionBlazor();

			services.AddProtectedBrowserStorage();

			services.AddBlazoredLocalStorage();

			services.AddSignalR(e =>
			{
				e.MaximumReceiveMessageSize = 102400000;
			});

			services.AddMatToaster(config =>
			{
				config.Position = MatToastPosition.BottomRight;
				config.PreventDuplicates = true;
				config.NewestOnTop = true;
				config.ShowCloseButton = true;
				config.MaximumOpacity = 95;
				config.VisibleStateDuration = 3000;
			});

			// Custom AuthenticationState provider
			services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
			services.AddScoped<CustomSettingsProvider>();

			AddVizlistServices(services);
		}

		private void AddVizlistServices(IServiceCollection services)
		{
			services.AddScoped<TagonomyExecutionDataStore>();
			services.AddScoped<ITagonomyExecutionDataStore>(serviceProvider => serviceProvider.GetService<TagonomyExecutionDataStore>());
			services.AddScoped<ILoggerService, LoggerServiceStub>();
			services.AddScoped<IInputHelper, InputHelper>();
			services.AddScoped<ITagonomyExecutionController, TagonomyExecutionController>();
			services.AddScoped<IActionDispatcher, PortalDesiActionDispatcher>();
			services.AddScoped<IKeyboardListener, KeyboardListenerStub>();
			services.AddScoped<IPopupService, PopupServiceStub>();
		}


		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			//Register Syncfusion license
			var syncfusionLicense = Configuration.GetValue<string>("SyncfusionLicense");
			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);

			// Do not delete
			var z = new MLStatisticsJobConfig();
			var x = new TagExportDataWizardConfig();

			ReflectionHelper.Instance.GatherExtraNativeAssemblies();

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

			//app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			// Authentication & Authorization
			app.UseAuthentication();
			app.UseAuthorization();

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
