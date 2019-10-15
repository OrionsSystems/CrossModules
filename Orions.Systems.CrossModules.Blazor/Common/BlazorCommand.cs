using Microsoft.AspNetCore.Components;
using Orions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.BlazorSample
{
	/// <summary>
	/// Provides conversion mechanism between the Blazor event system and the DefaultCommand pattern.
	/// </summary>
	public class BlazorCommand : DefaultCommand, IHandleEvent
	{
		public BlazorCommand()
		{
		}

		public static implicit operator EventCallback(BlazorCommand comm)
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
