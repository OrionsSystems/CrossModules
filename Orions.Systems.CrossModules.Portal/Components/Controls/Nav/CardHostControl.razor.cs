using Microsoft.AspNetCore.Components;

using Orions.Common;
using Orions.SDK;
using Orions.Systems.CrossModules.Portal.Domain;
using Orions.Systems.CrossModules.Components;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Components
{
	public partial class CardHostControl : BaseBlazorComponent
	{
		public CardControlVm CardVm { get; private set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
		}

		protected override void OnDataContextAssigned(object dataContext)
		{
			base.OnDataContextAssigned(dataContext);

			OnDataContextChanged();
		}

		protected override void OnStateHasChanged()
		{
			base.OnStateHasChanged();
		}

		private void OnDataContextChanged()
		{
			if (DataContext is NavigationEntryVm entryVm)
			{
				CardVm = new CardControlVm(entryVm);
				CardVm.StartingItem += Vm_StartingItem;
			}
			else {
				CardVm = null;
			}
		}

		private bool Vm_StartingItem(CardControlVm cardControlVm)
		{
			return true;
		}
	}
}
