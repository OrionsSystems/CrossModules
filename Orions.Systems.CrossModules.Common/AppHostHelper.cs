using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;

namespace Orions.CrossModules.Common
{
	/// <summary>
	/// Inherits AppHostHelper and implements Startup routines.
	/// </summary>
	/// <typeparam name="StartupType"></typeparam>
	public class AppHostHelper<StartupType> : AppHostHelper
		where StartupType : class
	{
		public AppHostHelper()
		{
		}

		protected override IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args)
				.UseKestrel()
				.UseStartup<StartupType>();
		}

		protected override IWebHostBuilder CreateWebHostBuilder(string[] args, string bindAddress, int bindPort)
		{
			return WebHost.CreateDefaultBuilder(args)
			  .UseKestrel()
			  .UseUrls(bindAddress + ":" + bindPort)
			  .UseStartup<StartupType>();
		}
	}

	/// <summary>
	/// Helps make work together the .net core app style initializer and the CrossModuleInstanceHost initialized.
	/// </summary>
	public abstract class AppHostHelper : Disposable
	{
		private readonly object _lock = new object();
		private bool _isWebHostInitialized;

		public string InstanceId => CrossModuleInstanceHost?.InstanceId;

		public CrossModuleInstanceHost CrossModuleInstanceHost { get; private set; }

		public CrossModuleConfig DevConfig { get; set; } = new CrossModuleConfig();

		public CrossModuleConfig AppliedConfig => CrossModuleInstanceHost?.AppliedConfig;

		public delegate void UpdateDelegate(AppHostHelper helper);

		public event UpdateDelegate OnReady;

		public AppHostHelper()
		{
		}

		protected abstract IWebHostBuilder CreateWebHostBuilder(string[] args);
		protected abstract IWebHostBuilder CreateWebHostBuilder(string[] args, string bindAddress, int bindPort);

		protected virtual void OnStarted()
		{
		}

		public void StartWithArgs(string[] args)
		{
			ConsoleHelper.WriteLine("Starting...");
			ConsoleHelper.WriteLine("Arguments are: " + string.Join(' ', args));

			Thread.Sleep(1000);

			if (CommonHelper.IsRunningAsService() == false)
			{
				var consoleLogger = new ConsoleLogger();
				consoleLogger.InitAsync(new ConsoleLoggerConfig()).GetAwaiter().GetResult();
				Logger.Instance.AddSink(consoleLogger);
			}

			Task.Factory.StartNew(async () =>
			{
				try
				{
					using (var host = new CrossModuleInstanceHost()
					{
						DevConfig = DevConfig
					})
					{
						CrossModuleInstanceHost = host;

						await host.InitAsync(args);

						//string nodeConnectionString = null;

						//if (host.AppliedConfig?.NodeConnections != null)
						//{
						//	var nodeConnectionStrings = host.AppliedConfig?.NodeConnections;

						//	if (nodeConnectionStrings != null && nodeConnectionStrings.Any())
						//	{
						//		var firstConnectionString = nodeConnectionStrings.First();

						//		if (!string.IsNullOrEmpty(firstConnectionString))
						//			nodeConnectionString = firstConnectionString;
						//	}
						//}

						IWebHost builderHost = null;

						if (host.AppliedConfig?.BindingPort == null)
						{
							if (host.AppliedConfig?.AutoSelectBindingPort == true)
							{
								Logger.Instance.Error(this, nameof(StartWithArgs), "****BINDING PORT NOT FOUND, STARTING ON AUTO***");

								builderHost = CreateWebHostBuilder(args).Build();
								_isWebHostInitialized = true;
							}
						}
						else
						{
							builderHost = CreateWebHostBuilder(args, host.AppliedConfig.BindingAddress, host.AppliedConfig.BindingPort.Value).Build();
							_isWebHostInitialized = true;
						}

						var t1 = Task.Factory.StartNew(() =>
						{
							Thread.Sleep(2000);
							OnStarted();
						}, TaskCreationOptions.LongRunning);

						OnReady?.Invoke(this);

						if (builderHost != null)
						{
							builderHost.Run();
							await host.Task;
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Instance.Error(typeof(AppHostHelper), nameof(StartWithArgs), ex);
				}
			});

			while (/*!_isWebHostInitialized && */this.KeepWorking)
			{
				Thread.Sleep(1000);
			}
		}
	}
}
