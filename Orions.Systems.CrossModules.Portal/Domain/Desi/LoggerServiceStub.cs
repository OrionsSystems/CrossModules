using Orions.Systems.Desi.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Domain.Desi
{
	public class LoggerServiceStub : ILoggerService
	{
		public void LogEvent(string eventName, string message)
		{
		}

		public void LogException(string message, Exception e)
		{
		}
	}
}
