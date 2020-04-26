using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Common.Services;
using Serilog;
using Serilog.Core;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class BlazorLoggerService : ILoggerService
	{
		private readonly Logger _logger;
		private readonly BlazorDependencyResolver dependencyResolver;

		public BlazorLoggerService(BlazorDependencyResolver dependencyResolver)
		{
			var logFileName = GetLogFilePath();
			this._logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.File(logFileName, rollingInterval: RollingInterval.Day)
				.CreateLogger();
			this.dependencyResolver = dependencyResolver;
		}

		private string GetLogFilePath()
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			return Path.Combine(Path.GetDirectoryName(path), "Logs", "desi-log.txt");
		}

		public void LogEvent(string eventName, string message)
		{
		}
		public void LogException(string message, Exception e)
		{
			var userName = GetCurrentUserName();
			_logger.Error(e, $"Exception thrown UserName: {userName}");
		}

		private string GetCurrentUserName()
		{
			var auth = dependencyResolver.GetAuthenticationSystem();

			string userName = null;
			var authInfo = auth?.Store?.Data?.AuthenticationInfo;

			if(authInfo != null)
			{
				switch (authInfo)
				{
					case HyperDomainAuthenticationInfo domain:
						userName = domain.Username;
						break;
					case HyperNodeAuthenticationInfo node:
						userName = node.Username;
						break;
				}
			}

			return userName;
		}
	}
}
