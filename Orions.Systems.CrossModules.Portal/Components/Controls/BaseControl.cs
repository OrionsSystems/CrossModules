using Microsoft.AspNetCore.Components;

using Orions.Systems.CrossModules.Components;
using Orions.Systems.CrossModules.Portal.Services;

using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Components
{
	public class BaseControl : BaseOrionsComponent
	{

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
		}
	}
}
