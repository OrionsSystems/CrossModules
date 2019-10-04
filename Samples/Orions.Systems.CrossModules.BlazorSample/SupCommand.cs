using Microsoft.AspNetCore.Components;
using Orions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.BlazorSample
{
	public class SupCommand : DefaultCommand, IHandleEvent
	{
		public SupCommand()
		{
		}

		public static implicit operator EventCallback(SupCommand comm)
		{
			return new EventCallback(comm, null);
		}

		public Task HandleEventAsync(EventCallbackWorkItem item, object arg)
		{
			this.Execute(null);
			return Task.CompletedTask;
		}
	}
}
