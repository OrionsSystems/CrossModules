using System;
using System.Linq;
using System.Threading.Tasks;

using Orions.Common;
using Orions.SDK;
using Orions.Infrastructure.Common;
using Orions.Systems.CrossModules.Components;
using Orions.Node.Common;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossModules.Portal.Services
{
	[Entity(typeof(SolutionEntity))]
	public class SolutionVmEx : SolutionVm
	{
		public BlazorCommand SaveCommand { get; set; } = new BlazorCommand();

		public BlazorCommand ChangeLayoutCommand { get; set; } = new BlazorCommand();

		public ViewModelProperty<bool> OverlayNotificationVisibleProp { get; set; } = new ViewModelProperty<bool>(false);

		public ViewModelProperty<string> OverlayNotificationTextProp { get; set; } = new ViewModelProperty<string>();

		DateTime _lastOverlayNotificationShown = DateTime.MinValue;

		public SolutionVmEx()
		{
			SaveCommand.Delegate = OnSave;
		}

		public override void BeginInvoke(Action action)
		{
			throw new NotImplementedException();
		}

		public override bool CheckAccess()
		{
			throw new NotImplementedException();
		}

		private void OnSave(DefaultCommand command, object parameter)
		{
			Save();
		}

		public override void ShowOverlayNotification(string message, bool severe = false)
		{
			throw new NotImplementedException();
		}

		public override void ShowWebPage(string title, string url, IModuleVm moduleVm, bool launchInExternalBrowser)
		{
			throw new NotImplementedException();
		}

		public override Task VisualizeInCrossModuleAsync(IModuleVm moduleVm, IHyperArgsSink hyperStore, CrossModuleVisualizationRequest request, HyperDocumentId moduleContainerId, string pageTitle, bool? showInExternalBrowser)
		{
			throw new NotImplementedException();
		}

		public override void OnSave()
		{
			Logger.Instance.HighPriorityInfo(this, nameof(OnSave), "Saving...");

		}

		protected override void OnEntityAssigned(INotifyProperty<SolutionEntity> prop, SolutionEntity newValue)
		{
			base.OnEntityAssigned(prop, newValue);

			if (newValue == null)
				return;

			var solutionEntity = newValue as SolutionEntity;
			if (solutionEntity?.ShowIntro == true)
			{
				// Show intro
				this.OverlayControlVisibleProp.Value = true;
				
				//TODO
			}

			//NavigationPanelVisibleProp.TakesFrom(newValue, nameof(SolutionEntity.Layout), (a, b) => (SolutionEntity.Layouts)b == SolutionEntity.Layouts.Default);
			//TabRowVisibleProp.TakesFrom(newValue, nameof(SolutionEntity.Layout), (a, b) => (SolutionEntity.Layouts)b != SolutionEntity.Layouts.Full2);

			// Load things only once we have the solution entity.
			//var tabHome = new HomePageVm();
			//AddTab(tabHome, false);


			// Auto load modules from classes.
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

			// Load things only once we have the solution entity.
			//var tabHome = new HomePageVm();
			//tabHome.ModuleVmProp.Value = Modules.OfType<HomeModuleVm>().FirstOrDefault();
			//tabHome.ModuleVmProp.Value.SolutionVmProp.Value = this;

			//AddTab(tabHome, false);

		}


	}
}
