using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TaskExploitation;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;

namespace Orions.Systems.CrossModules.Desi.Components.TaskNavigation
{
	public class TaskNavigationWidgetBase : BaseComponent
	{
		private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

		private ITaskDataStore _store;

		[Inject]
		public ITaskDataStore Store { get; set; }

		[Inject]
		public IActionDispatcher ActionDispatcher { get; set; }

		protected TaskExploitationData Data => Store.Data;

		protected void GoToNextTask() => ActionDispatcher.Dispatch(GoToNextTaskAction.Create());
		protected void GoToPrevTask() => ActionDispatcher.Dispatch(GoToPreviousTaskAction.Create());
		protected void SetCurrentTask(TaskModel task) => ActionDispatcher.Dispatch(SetCurrentTaskAction.Create(task));

		protected override void OnInitializedSafe()
		{
			base.OnInitializedSafe();
			_subscriptions.AddItem(Store.CurrentTaskChanged.Subscribe(_ => UpdateState()))
				.AddItem(Store.Data.GetPropertyChangedObservable()
					.Where(i => i.EventArgs.PropertyName == nameof(TaskExploitationData.Tasks))
					.Subscribe(_ => UpdateState()));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_subscriptions.ForEach(i => i.Dispose());
				_subscriptions.Clear();
			}
			base.Dispose(disposing);
		}
	}
}
