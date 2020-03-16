using Orions.Systems.Desi.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Debug.Infrastructure
{
	public class BlazorDispatcher : IDispatcher
	{
		public void Invoke(Action action)
		{
			action.Invoke();
		}

		public async Task InvokeAsync(Func<Task> action)
		{
			await action.Invoke();
		}

		public async Task<T> InvokeAsync<T>(Func<Task<T>> action)
		{
			return (await action());
		}

		public bool IsInvokeRequired()
		{
			throw new NotImplementedException();
		}
	}
}
