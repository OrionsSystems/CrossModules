using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orions.SDK;

using Microsoft.AspNetCore.Components;

using Orions.Systems.CrossModules.Components;
using Orions.Systems.CrossModules.Portal.Services;


namespace Orions.Systems.CrossModules.Portal.Shared
{
	public class LeftSideMenuBase : BaseOrionsComponent
	{
		private List<NavigationEntryVm> Items = new List<NavigationEntryVm>();

		[CascadingParameter]
		protected SolutionVmEx Solution { get; set; }

		[Parameter]
		public bool Enable { get; set; }

		[Parameter]
		public IModuleVm Module { get; set; }

		protected override void OnInitialized()
		{
			base.OnInitialized();
		}

		protected override void OnParametersSet()
		{
			base.OnParametersSet();

			Items.Clear();

			if (Module != null)
			{
				//var connections = Module.SolutionVmProp.Value?.GetModuleVm<ActiveCardsModuleVm>();

				//Items = connections?.CurrentEntries?.ToList() ?? new List<NavigationEntryVm>();

			}
		}
	}
}
