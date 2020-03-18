using Microsoft.AspNetCore.Components;
using Orions.Desi.Forms.Core.Services;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi
{
	public class AppComponentBase : ComponentBase
	{
		[Inject]
		public BlazorDependencyResolver DependencyResolver { get; set; }
		public bool IsSettingsInitialized { get; private set; }

		protected override async Task OnInitializedAsync()
		{
			var settingsStorage = DependencyResolver.GetSettingsStorage() as BrowserLocalSettingsStorage;

			await settingsStorage.Load();

			IsSettingsInitialized = true;
		}
	}
}
