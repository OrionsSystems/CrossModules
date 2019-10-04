using Orions.Common;
using System;
using System.Linq;
using Orions.Infrastructure.HyperMedia;
using System.Threading.Tasks;
using Orions.Common.ServiceBus;
using Orions.Services.Contracts;

namespace Orions.CrossModuleTestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Starting...");
			Console.WriteLine("Arguments are: " + string.Join(' ', args));

			Console.WriteLine("Force-loading: " + typeof(PingEvent));

			var consoleLogger = new ConsoleLogger();
			consoleLogger.InitAsync(new ConsoleLoggerConfig()).GetAwaiter().GetResult();
			Logger.Instance.AddSink(consoleLogger);

			if (args.Length > 1 && args[0] == "-crossModule")
			{
				Task.Factory.StartNew(async () =>
				{
					try
					{
						using (var host = new CrossModuleInstanceHost()
						{
							DevConfig = new CrossModuleConfig()
						})
						{
							await host.InitAsync(args);

							host.ControlBusNode.Subscribe<IMessage>("System", callbackDelegate);

							Logger.Instance.HighPriorityInfo(typeof(Program), nameof(Main), "Binding port: " + host.AppliedConfig.BindingPort);

							await host.Task;

							//var assets = await host.NodesConnections.First().ExecuteAsyncThrows(new RetrieveAssetsArgs());
							//Logger.Instance.HighPriorityInfo(typeof(Program), nameof(Main), "Retrieved assets: " + assets);
						}
					}
					catch (Exception ex)
					{
						Logger.Instance.Error(typeof(Program), nameof(Main), ex);
					}
				});
			}

			Console.WriteLine("Waiting... press Esc to stop app");
			while (true)
			{
				var key = Console.ReadKey();
				if (key.Key == ConsoleKey.Escape)
					break;
			}

			ApplicationLifetimeHelper.SetApplicationClosing();
			Console.WriteLine("Exiting...");
		}

		private static void callbackDelegate(BusNode arg1, string arg2, IMessage arg3)
		{
			Console.WriteLine("Message received on system channel: " + arg3);
		}
	}
}
