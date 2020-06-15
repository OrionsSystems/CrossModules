using Microsoft.AspNetCore.Components;

using Orions.Common;
using Orions.SDK;
using Orions.Systems.CrossModules.Components;
using Orions.Systems.CrossModules.Portal.Helpers;

using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Components
{
	[ViewModel(typeof(CardControlVm), false)]
	public class CardControlBase : BaseOrionsComponent
	{
		[CascadingParameter]
		protected NavigationEntryVm Source { get; set; }

		public CardControlVm CardControl { get; set; } 

		public bool IsCompactMode
		{
			get
			{
				if (Source == null) return true;

				return Source.IsCompactModeProp.Value;
			}
		}

		protected void OnClickCardControl()
		{
			var ctr = new CardControlVm(Source);

			ctr.StartItem(null);

			StateHasChanged();
		}

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
		}

		protected string GetCustomStatusBackgroudColor()
		{

			if (Source.HasCustomStatusBrushProp.Value)
			{
				var color = Source.CustomStatusBrushProp.Value;
				return $"background-color: {PredefinedCustomColorHelper.GetColor(color)};";
			}

			return string.Empty;
		}

		protected string GetBorderClass()
		{
			if (Source == null) return "brd-default";

			if (Source.IsCreator)
				return "brd-creator";

			if (Source.IsItemCreator)
				return "brd-item-creator";

			if (Source.IsTabCreator)
				return "brd-tab-creator";

			return "brd-default";
		}


		//public void StartItem(ConnectionVm selectedConnection)
		//{
		//	//if (selectedConnection == null && StartingItem?.Invoke(this) == false)
		//	//	return;

		//	var entryVm = EntryVmProp.Value;

		//	if (entryVm is IConnectedVm connectedVm && selectedConnection != null)
		//	{
		//		connectedVm.AllowedConnectionIds.SetTo(new string[] { selectedConnection.EntityId });
		//	}

		//	entryVm?.EventHandler?.ItemLaunchCommand.Execute(entryVm);
		//}
	}
}
