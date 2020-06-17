using Microsoft.AspNetCore.Components;

using Orions.Common;
using Orions.SDK;
using Orions.Systems.CrossModules.Portal.Domain;
using Orions.Systems.CrossModules.Components;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Components
{
	public class CardHostControlBase : BaseBlazorComponent
	{

		public CardControlVm CardControl { get; private set; }

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
				CardControl = new CardControlVm(entryVm);
				CardControl.StartingItem += Vm_StartingItem;
			}
			else { 
			
			}
		}

		private bool Vm_StartingItem(CardControlVm cardControlVm)
		{
			return true;
		}
	}
}
