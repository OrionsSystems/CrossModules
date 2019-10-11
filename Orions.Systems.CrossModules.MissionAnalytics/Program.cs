﻿using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Common;
using System.Linq;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class Program
	{
		static AppHostHelper<ModuleStartup> _helper;

		public static CrossModuleInstanceHost CrossModuleInstanceHost => _helper?.CrossModuleInstanceHost;

		/// <summary>
		/// Main entry point for the application.
		/// </summary>
		public static void Main(string[] args)
		{
			_helper = new AppHostHelper<ModuleStartup>();

			_helper.OnReady += OnReadyHandler;

			_helper.StartWithArgs(args);
		}

		private static async void OnReadyHandler(AppHostHelper helper)
		{
			await DataContext.Instance.InitStoreAsync(helper?.AppliedConfig?.NodeConnections.FirstOrDefault());
		}
	}
}
