using System;

using Orions.Common;
using Orions.CrossModules.Common;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossModules.NodeLite
{
	class Program
	{
		static NodeLiteHelper _helper;

		public static CrossModuleInstanceHost CrossModuleInstanceHost => _helper?.CrossModuleInstanceHost;
		
		static void Main(string[] args)
		{
			_helper = new NodeLiteHelper();

			// No Web server
			_helper.DevConfig.BindingPort = null;
			_helper.DevConfig.AutoSelectBindingPort = false;

			_helper.DevConfig.CloudConnectionString = "";
			//_helper.DevConfig.NodeConnections = new string[] { };

			_helper.StartWithArgs(args);
		}


		public class NodeLiteHelper : AppHostHelper<CrossModuleStartup>
		{
			public NodeLiteHelper()
			{
			}

			/// <summary>
			/// https://stackoverflow.com/questions/570098/in-c-how-to-check-if-a-tcp-port-is-available
			/// </summary>
			protected bool CheckPortFree(int port)
			{
				// Evaluate current system tcp connections. This is the same information provided
				// by the netstat command line application, just in .Net strongly-typed object
				// form.  We will look through the list, and if our port we would like to use
				// in our TcpClient is occupied, we will set isAvailable to false.
				var ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
				var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

				foreach (var tcpi in tcpConnInfoArray)
				{
					if (tcpi.LocalEndPoint.Port == port)
					{
						return false;
					}
				}

				return true;
			}

			protected override void OnStarted()
			{// Start the Node.

				CommonHelper.SDKMode = false; // Import to enfore the license, this needs to run BEFORE the GatherExtraNativeAssemblies.
				JsonHelper.ThrowOnError = false;

				// Force load
				var z = new HyperServerMasterConfig(); // Orions.Node.SDK

				if (string.IsNullOrEmpty(this.InstanceId))
					throw new OrionsException("Failed to establish proper instanceId");

				Logger.Instance?.VeryHighPriorityInfo(typeof(Program), nameof(Main), "Starting NodeLite...");

				string yamlTemplate = CommonHelper.ReadEmbeddedResourceText(this.GetType().Assembly, "Orions.CrossModule.NodeLite.ReactNOW.Node.Lite.Template.yml");
				yamlTemplate = yamlTemplate.Replace("{id}", this.InstanceId);

				int? appliedPort = this.AppliedConfig.DynamicParameters.TryGetIntDynamicParameter("HttpPort");
				if (appliedPort.HasValue == false)
				{// Auto find a free port.
					var rand = new Random();
					for (int i = 0; i < 50; i++)
					{
						int port = rand.Next(7000, 8000);
						if (CheckPortFree(port))
						{
							appliedPort = port;
							break;
						}
					}
				}

				if (appliedPort.HasValue == false)
				{
					Logger.Instance?.VeryHighPriorityInfo(typeof(Program), nameof(Main), "*** FAILED TO FIND AN OPEN PORT FOR NETSTORE SERVER ***");
					return;
				}

				yamlTemplate = yamlTemplate.Replace("{HttpPort}", appliedPort.Value.ToString());

				string folder = this.AppliedConfig.DynamicParameters.TryGetStringDynamicParameter("FileStoreFolder");
				yamlTemplate = yamlTemplate.Replace("{FileStoreFolder}", folder ?? "");

				// Console mode.
				var server = ElementServerFactory.CreateFromYaml(yamlTemplate);
				server.Run(true);

				base.OnStarted();
			}
		}
	}
}
