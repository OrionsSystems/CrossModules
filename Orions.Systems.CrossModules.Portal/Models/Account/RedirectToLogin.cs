using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Models
{
	public class RedirectToLogin : ComponentBase
	{
		[Inject]
		protected NavigationManager NavigationManager { get; set; }

		protected override void OnInitialized()
		{
			NavigationManager.NavigateTo("login");
		}
	}
}
