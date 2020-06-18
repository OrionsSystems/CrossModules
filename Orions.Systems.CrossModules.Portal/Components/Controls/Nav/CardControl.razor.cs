
using Orions.Common;
using Orions.SDK;
using Orions.Systems.CrossModules.Components;
using Orions.Systems.CrossModules.Portal.Helpers;

using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Components
{
	[ViewModel(typeof(CardControlVm), true)]
	public partial class CardControl : BaseBlazorComponent
	{
		protected CardControlVm CardVm { get { return (CardControlVm)DataContext; } }

		protected NavigationEntryVm Source { get { return CardVm?.EntryVmProp.Value; } }

		public bool IsCreatorMode { get; private set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
		}

		protected override void OnDataContextAssigned(object dataContext)
		{
			base.OnDataContextAssigned(dataContext);

			OnDataContextChanged();
		}

		private void OnDataContextChanged()
		{

			UpdateCreatorMode(CardVm.EntryVmProp.Value.HasMoreProp.Value);

			//TODO create context menu

			StateHasChanged();
		}

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
			if (CardVm != null)
			{
				CardVm.StartItem(null);
			}

			StateHasChanged();
		}

		protected void OnPropertyClick()
		{
			var cmd = Source.EventHandler.ItemPropertiesCommand;
			cmd.Execute(Source);
		}

		protected void OnActiveClick()
		{
			Source.ActiveProp.Value = !Source.ActiveProp.Value;
		}

		protected void OnLucnhTabClick()
		{
			var cmd = Source.EventHandler.ItemLaunchTabCommand;
			cmd.Execute(Source);
		}

		protected void OnExpandClick()
		{
			var cmd = Source.EventHandler.ItemLaunchExpandCommand;
			cmd.Execute(Source);
		}

		protected void OnFavouriteClick()
		{
			var cmd = Source.EventHandler.ItemFavouritesCommand;
			cmd.Execute(Source);
		}

		protected void OnHelpClick()
		{
			var cmd = Source.EventHandler.ItemHelpCommand;
			cmd.Execute(Source);
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

		private void UpdateCreatorMode(bool hasNewPropValue = false)
		{
			var isCreator = CardVm?.EntryVmProp.Value is CreatorEntryVm;
			if (hasNewPropValue || isCreator)
			{
				IsCreatorMode = true;
				return;
			}

			IsCreatorMode = false;
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
