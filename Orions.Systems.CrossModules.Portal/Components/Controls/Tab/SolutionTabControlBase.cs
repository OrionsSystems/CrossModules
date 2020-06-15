using Microsoft.AspNetCore.Components;

using Orions.Common;
using Orions.SDK;
using Orions.Systems.CrossModules.Portal.Domain;
using Orions.Systems.CrossModules.Components;

using System.Threading.Tasks;
using Orions.Systems.CrossModules.Portal.Services;

namespace Orions.Systems.CrossModules.Portal.Components
{
	public class SolutionTabControlBase : BaseOrionsComponent
	{
		[Parameter]
		public SolutionVmEx	Solution { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
		}
	}
}
