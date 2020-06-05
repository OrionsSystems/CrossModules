using Microsoft.AspNetCore.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orions.Systems.CrossModules.Portal;
using Orions.Systems.CrossModules.Portal.Providers;
using Microsoft.AspNetCore.Components.Authorization;
using Syncfusion.EJ2.Blazor.Navigations;
using Microsoft.AspNetCore.WebUtilities;
using MatBlazor;
using Orions.Common;
using Orions.SDK;
using Orions.Infrastructure.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class MissionListBase : LayoutComponentBase
	{
		object _syncRoot = new object();

		[Inject]
		public NavigationManager NavigationManager { get; set; }


		[Inject]
		AuthenticationStateProvider AuthenticationStateProvider { get; set; }

		[Inject]
		CustomSettingsProvider CustomSettingsProvider { get; set; }

		[Inject]
		NavigationManager navigationManager { get; set; }

		public AdvancedObservableCollection<ModuleVm> Modules { get; private set; } = new AdvancedObservableCollection<ModuleVm>();
		public AdvancedObservableCollection<ModuleVm> OptionalModules { get; private set; }
		public ModuleVm DefaultDynamicModuleVm => ActiveModules.FirstOrDefault();
		public AdvancedObservableCollection<ModuleVm> ActiveModules { get; private set; }
		public AdvancedObservableCollection<ModuleVm> VisibleModules { get; private set; }

		protected string VersionLabel { get; set; }

		protected List<NavMenuItem> NavItems = new List<NavMenuItem> {
			new NavMenuItem{ Address="", Label="Home", Alias="properties", MatIcon=MatIconNames.Home },
			new NavMenuItem{ Address="dashboards", Label="Dashboards", MatIcon=MatIconNames.View_module },
			new NavMenuItem{ Address="themes", Label="Themes", MatIcon=MatIconNames.Color_lens},
			new NavMenuItem{ Address="wizzard", Label="Wizzard", MatIcon=MatIconNames.Widgets},
			new NavMenuItem{ Address="missions", Label="Missions", MatIcon=MatIconNames.Slideshow, EnableLeftMenu=false },
			new NavMenuItem{ Address="workflows", Label="Workflows", MatIcon=MatIconNames.Library_books},
			new NavMenuItem{ Address="tagonomies", Label="Tagonomies", MatIcon=MatIconNames.Extension,  EnableLeftMenu=false }
		};

		private NavMenuItem SelectedNavItem { get; set; }

		protected bool IsEnableLeftMenu { get { return SelectedNavItem?.EnableLeftMenu ?? false; } }

		protected EjsSidebar dockSidebarInstance { get; set; }

		protected string CurrentTheme { get; set; }

		protected bool IsEmbedded { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			VersionLabel = ReflectionHelper.Instance.SDKVersion;

			PopulateModules();

			Modules.SetOrderByDelegate((a) => a.Order);

			OptionalModules = new AdvancedObservableCollection<ModuleVm>(this.Modules, it => it.Optional, it => it.Order, "Optional_Modules")
			{
				//OrderByDescending = true
			};

			ActiveModules = new AdvancedObservableCollection<ModuleVm>(this.Modules, it => it.IsActiveProp.Value, it => it.Order, "Active_Modules")
			{
				//OrderByDescending = true
			}; // Non optional ones are always active

			VisibleModules = new AdvancedObservableCollection<ModuleVm>(this.Modules, it => it.Visible && it.IsActiveProp.Value, it => it.Order, "Visible_Modules")
			{
				//OrderByDescending = true
			}; // Non optional ones are always active

			SelectedNavItem = FindNavItem();

			CurrentTheme = await GetTheme();

			var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
			if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("embed", out var token))
			{
				IsEmbedded = true;
			}

			if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("theme", out var value))
			{
				CurrentTheme = value;
			}
		}

		private void PopulateModules() 
		{
			// Auto load modules from classes.
			var solutionEntity = new SolutionEntity();

			foreach (Type moduleEntityType in ReflectionHelper.Instance.GatherTypeChildrenTypesFromAssemblies(typeof(ModuleEntity), ReflectionHelper.Instance.AllAssemblies))
			{
				if (moduleEntityType.IsAbstract || moduleEntityType.IsClass == false)
					continue;

				var profilesAttr = (AppProfilesAttribute[])moduleEntityType.GetCustomAttributes(typeof(AppProfilesAttribute), true);
				if (profilesAttr?.Length > 0 && solutionEntity.AppProfile != Orions.Infrastructure.Common.AppProfiles.All && profilesAttr.Any(it => it.Profile == solutionEntity.AppProfile) == false)
				{
					solutionEntity.RemoteModuleEntityType(moduleEntityType);
					continue; // Was marked with profiles attribute, but not compatible with this.
				}

				ModuleEntity moduleEntity = solutionEntity.ModulesArray.FirstOrDefault(it => it.GetType() == moduleEntityType);
				if (moduleEntity == null)
				{
					moduleEntity = (ModuleEntity)solutionEntity.ObtainModuleEntity(moduleEntityType);
				}
			}

			// The starting priority is important, as we need to fire up connections etc. before the others.
			var modules = solutionEntity.ModulesArray.OrderByDescending(it => it.StartingPriority).ToArray();
			foreach (ModuleEntity moduleEntity in modules)
			{// Existing modules.
				var module = TryObtainModuleVmByEntity(moduleEntity);
			}
		}

		public ModuleVm TryObtainModuleVmByEntity(ModuleEntity entity)
		{
			var module = this.Modules.FirstOrDefault(it => it.Entity == entity);
			if (module != null)
				return module;

			Type vmType = EntityAttribute.FindTypeByConfigType<ModuleVm>(entity.GetType(), false, true);
			if (vmType == null)
			{
				//System.Diagnostics.Debug.Assert(false, "Module Entity has no view model type: " + entity);
				return null;
			}

			module = ObtainModuleVm(vmType);
			//module.SolutionVmProp.Value = this;
			module.EntityProp.Value = entity;

			return module;
		}

		public ModuleVm ObtainModuleVm(Type moduleType)
		{
			ModuleVm result;
			lock (_syncRoot)
			{// This is a semi lock case.
				result = (ModuleVm)Modules.FirstOrDefault(it => moduleType.IsAssignableFrom(it.GetType()));
				if (result != null)
					return result;

				result = (ModuleVm)Activator.CreateInstance(moduleType);
				Modules.Add(result); // Events in lock?!?
			}

			return result;
		}

		private NavMenuItem FindNavItem()
		{
			var currentaddress = navigationManager.ToBaseRelativePath(navigationManager.Uri).ToString();

			var data = NavItems.Where(it => currentaddress.Contains(string.IsNullOrWhiteSpace(it.Alias) ? it.Address : it.Alias)).FirstOrDefault();

			if (data == null)
			{
				data = NavItems.FirstOrDefault();
			}

			return data;
		}

		private async Task Logout()
		{
			await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).LogOut();

			NavigationManager.NavigateTo("/login");
		}

		private async Task<string> GetTheme()
		{
			var theme = await CustomSettingsProvider.GetParameter(CustomSettingsProvider.THEME_KEY);

			if (string.IsNullOrWhiteSpace(theme)) return CustomSettingsProvider.THEME_WHITE;

			return theme;
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

		public async Task OnItemSelected(Syncfusion.EJ2.Blazor.SplitButtons.MenuEventArgs args)
		{
			if (args.Element.ID.Equals("Logout", StringComparison.InvariantCultureIgnoreCase))
			{
				await Logout();
			}
		}
	}
}
