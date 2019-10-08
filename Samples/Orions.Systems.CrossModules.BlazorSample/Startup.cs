using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Orions.Systems.CrossModules.BlazorSample.Data;
using Orions.Systems.CrossModules.Common;

namespace Orions.Systems.CrossModules.BlazorSample
{
	public class Startup : BlazorCrossModuleStartup
	{
		public Startup(IConfiguration configuration)
			: base(configuration)
		{
		}

		public override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services.AddSingleton<TestService>();
		}

		public override void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			base.Configure(app, env);
		}

	}
}