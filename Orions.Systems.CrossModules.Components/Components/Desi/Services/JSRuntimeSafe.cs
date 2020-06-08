using Microsoft.JSInterop;
using Orions.Systems.Desi.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components.Desi.Services
{
	public interface IJSRuntimeSafe : IJSRuntime { }

	public class JSRuntimeSafe : IJSRuntimeSafe
	{
		private readonly IJSRuntime _jsRuntime;
		private readonly ILoggerService _logger;

		public JSRuntimeSafe(IJSRuntime jsRuntime, ILoggerService logger)
		{
			_jsRuntime = jsRuntime;
			_logger = logger;
		}

		public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args)
		{
			try
			{
				return _jsRuntime.InvokeAsync<TValue>(identifier, args);
			}
			catch(Exception e)
			{
				_logger?.LogException("JSRuntime exception", e);
				throw;
			}
		}

		public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object[] args)
		{
			try
			{
				return _jsRuntime.InvokeAsync<TValue>(identifier, cancellationToken, args);
			}
			catch (Exception e)
			{
				_logger?.LogException("JSRuntime exception", e);
				throw;
			}
		}
	}
}
