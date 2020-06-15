using MatBlazor;

using Microsoft.AspNetCore.Components;

using Orions.Systems.CrossModules.Portal;
using Orions.Systems.CrossModules.Portal.Domain;
using Orions.Systems.CrossModules.Portal.Providers;

using Syncfusion.EJ2.Blazor.Navigations;
using Orions.Systems.CrossModules.Portal.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal
{
	public class LayoutBase : PortalLayoutComponent
	{

		protected List<NavMenuItem> NavItems = new List<NavMenuItem>
		{
			new NavMenuItem{ Address="", Label="Home", Alias="properties", MatIcon=MatIconNames.Home },
			new NavMenuItem{ Address="dashboards", Label="Dashboards", MatIcon=MatIconNames.View_module},
			new NavMenuItem{ Address="themes", Label="Themes", MatIcon=MatIconNames.Color_lens},
			new NavMenuItem{ Address="wizzard", Label="Wizzard", MatIcon=MatIconNames.Widgets},
			new NavMenuItem{ Address="missions", Label="Missions", MatIcon=MatIconNames.Slideshow },
			new NavMenuItem{ Address="workflows", Label="Workflows", MatIcon=MatIconNames.Library_books},
			new NavMenuItem{ Address="tagonomies", Label="Tagonomies", MatIcon=MatIconNames.Extension }
		};

		private NavMenuItem SelectedNavItem { get; set; }

		protected bool IsEnableLeftMenu { get { return SelectedNavItem?.EnableLeftMenu ?? false; } }

		protected EjsSidebar dockSidebarInstance { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			MappingModuleToNavigation();

			SelectedNavItem = FindNavItem();
		}

		private void MappingModuleToNavigation()
		{
			if (Solution != null && !Solution.VisibleModules.Any()) return;

			foreach (var mod in Solution.VisibleModules)
			{
				var imgSrc = mod.ImageSourceProp.Value?.DataAsBase64();
				if (imgSrc != null)
				{
					imgSrc = $"data:image/png;base64, {imgSrc}";
				}

				NavItems.Add(
					new NavMenuItem
					{
						Source = mod,
						Address = $"module/{mod.Signature}",
						Label = mod.Signature,
						ImageSource = imgSrc,
						Description = mod.HelpText,
						MatIcon = MatIconNames.Developer_board,
						EnableLeftMenu = true
					}
				);
			}
		}

		private NavMenuItem FindNavItem()
		{
			var currentaddress = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToString();

			if (string.IsNullOrWhiteSpace(currentaddress)) return null;

			var data = NavItems.Where(it => currentaddress.Contains(string.IsNullOrWhiteSpace(it.Alias) ? it.Address : it.Alias)).FirstOrDefault();

			if (data == null)
			{
				data = NavItems.FirstOrDefault();
			}

			return data;
		}

		public async Task OnItemSelected(Syncfusion.EJ2.Blazor.SplitButtons.MenuEventArgs args)
		{
			if (args.Element.ID.Equals("Logout", StringComparison.InvariantCultureIgnoreCase))
			{
				await Logout();
			}
		}

		public async Task OnThemeSelected(Syncfusion.EJ2.Blazor.SplitButtons.MenuEventArgs args)
		{
			if (args.Element.ID.Equals(CustomSettingsProvider.THEME_DARK, StringComparison.InvariantCultureIgnoreCase))
			{
				await CustomSettingsProvider.SetParameter(CustomSettingsProvider.THEME_KEY, CustomSettingsProvider.THEME_DARK);
				CurrentTheme = CustomSettingsProvider.THEME_DARK;
			}

			if (args.Element.ID.Equals(CustomSettingsProvider.THEME_WHITE, StringComparison.InvariantCultureIgnoreCase))
			{
				await CustomSettingsProvider.SetParameter(CustomSettingsProvider.THEME_KEY, CustomSettingsProvider.THEME_WHITE);
				CurrentTheme = CustomSettingsProvider.THEME_WHITE;
			}
		}

		protected void ForceNavigation(NavMenuItem item)
		{
			var vm = item.Source;
			if (vm != null) {
				vm.SelectedCommand.Execute(null);
			}

			var address = item.Address ?? String.Empty;
			var url = $"{NavigationManager.BaseUri}{address}";
			NavigationManager.NavigateTo(url, item.ForceReload);

			SelectedNavItem = item;

			StateHasChanged();
		}

	}
}
