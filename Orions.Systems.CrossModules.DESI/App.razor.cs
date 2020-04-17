using Microsoft.AspNetCore.Components;
using Orions.Desi.Forms.Core.Services;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi
{
	public class AppComponentBase : BaseComponent
	{
		[Inject]
		public BlazorDependencyResolver DependencyResolver { get; set; }
		public bool IsSettingsInitialized { get; private set; }

		protected override async Task OnInitializedAsyncSafe()
		{
			var settingsStorage = DependencyResolver.GetSettingsStorage() as BrowserLocalSettingsStorage;

			await settingsStorage.Load();

			IsSettingsInitialized = true;
		}
	}
}
