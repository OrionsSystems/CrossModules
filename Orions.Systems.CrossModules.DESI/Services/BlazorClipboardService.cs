using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Services;
using Microsoft.JSInterop;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class BlazorClipboardService : IDeviceClipboardService
	{
		private readonly IJSRuntime _jSRuntime;

		public BlazorClipboardService(IJSRuntime jSRuntime)
		{
			_jSRuntime = jSRuntime;
		}

		public async Task SetTextAsync(string text)
		{
			await _jSRuntime.InvokeVoidAsync("Orions.ClipboardService.setText", new object[] { text });
		}
	}
}
