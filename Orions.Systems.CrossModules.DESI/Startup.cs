using Blazored.LocalStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orions.Desi.Forms.Core.Services;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.CrossModules.Desi.Services;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Media;
using Orions.Systems.Desi.Common.MissionsExploitation;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.Desi.Common.Tagging;
using Orions.Systems.Desi.Common.TagonomyExecution;
using Orions.Systems.Desi.Common.TagsExploitation;
using Orions.Systems.Desi.Common.TaskExploitation;
using Orions.Systems.Desi.Core;
using Syncfusion.EJ2.Blazor;
using Orions.Systems.CrossModules.Components.Desi.Services;
using Orions.Systems.Desi.Common.Tracking;

namespace Orions.Systems.CrossModules.Desi
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
			services.AddServerSideBlazor();
			services.AddBlazoredLocalStorage();

			services
				.AddScoped<BlazorDependencyResolver>()
				.AddScoped<IDependencyResolver>(s => s.GetService<BlazorDependencyResolver>())
			;

			AddViewModels(services);
			AddServices(services);
			AddSystems(services);
			AddPlatformServices(services);

			services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });
			services.AddSyncfusionBlazor();
			var syncfusionLicense = Configuration.GetValue<string>("SyncfusionLicense");
			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);

			services.AddScoped<ILoggerService, BlazorLoggerService>();
		}

		private void AddViewModels(IServiceCollection services)
		{
			services.AddScoped<ViewModelLocator>()
				.AddScoped(s => s.GetService<ViewModelLocator>().GetAuthenticationViewModel())
				.AddScoped(s => s.GetService<ViewModelLocator>().GetMissionsViewModel())
				.AddScoped(s => s.GetService<ViewModelLocator>().GetStageSelectionViewModel())
				.AddTransient(s => s.GetService<ViewModelLocator>().GetTaggingViewModel())
				;
		}

		private void AddPlatformServices(IServiceCollection services)
		{
			services.AddScoped<ILocalStorageService, LocalStorageService>()
				.AddScoped<IKeyboardListener, KeyboardListener>()
				.AddScoped<IPopupService, PopupService>()
				;
		}

		private void AddServices(IServiceCollection services)
		{
			services.AddScoped<INavigationService>(s => s.GetService<BlazorDependencyResolver>().GetNavigationService())
				.AddScoped<IFrameCacheService>(s => s.GetService<BlazorDependencyResolver>().GetFrameCacheService())
				.AddScoped<INetStoreProvider>(s => s.GetService<BlazorDependencyResolver>().GetNetStoreProvider())
				.AddScoped<ISettingsStorage>(s => s.GetService<BlazorDependencyResolver>().GetSettingsStorage())
				;
		}

		private void AddSystems(IServiceCollection services)
		{
			services.AddScoped<IAuthenticationSystem>(s => s.GetService<BlazorDependencyResolver>().GetAuthenticationSystem())
				.AddScoped<IMissionsExploitationSystem>(s => s.GetService<BlazorDependencyResolver>().GetMissionsExploitationSystem())
				.AddScoped<ITaggingSystem>(s => s.GetService<BlazorDependencyResolver>().GetTaggingSystem())
				
				.AddScoped<ITagsStore>(s => s.GetService<ITaggingSystem>().TagsStore)
				.AddScoped<ITaskDataStore>(s => s.GetService<ITaggingSystem>().TaskDataStore)
				.AddScoped<ITagonomyExecutionDataStore>(s => s.GetService<ITaggingSystem>().TagonomyExecutionDataStore)
				.AddScoped<IMediaDataStore>(s => s.GetService<ITaggingSystem>().MediaDataStore)
				.AddScoped<ITrackingDataStore>(s => s.GetService<ITaggingSystem>().TrackingDataStore)
				.AddScoped<IActionDispatcher>(s => s.GetService<ITaggingSystem>().ActionDispatcher)
				;
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
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
		}
	}
}
