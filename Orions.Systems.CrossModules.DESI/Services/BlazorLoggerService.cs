using System;
using System.Diagnostics;
using Orions.Systems.Desi.Common.Services;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class BlazorLoggerService : ILoggerService
	{
		public void LogEvent(string eventName, string message) => Debug.WriteLine($"{eventName}:{message}");
		public void LogException(string message, Exception e) => Debug.WriteLine($"{message}:{e}");
	}
}
