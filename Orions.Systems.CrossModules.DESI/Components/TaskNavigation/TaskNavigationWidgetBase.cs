using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TaskExploitation;

namespace Orions.Systems.CrossModules.Desi.Components.TaskNavigation
{
	public class TaskNavigationWidgetBase : BaseComponent
	{
		private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

		private ITaskDataStore _store;

		[Parameter]
		public ITaskDataStore Store
		{
			get => _store;
			set => SetProperty(ref _store,
				value,
				() =>
				{
					_subscriptions.AddItem(value.CurrentTaskChanged.Subscribe(_ => UpdateState()))
					.AddItem(value.Data.Tasks.GetCollectionChangedObservable().Subscribe(_ => UpdateState()));
				});
		}

		[Parameter]
		public IActionDispatcher ActionDispatcher { get; set; }

		protected TaskExploitationData Data => Store.Data;

		protected void GoToNextTask() => ActionDispatcher.Dispatch(GoToNextTaskAction.Create());
		protected void GoToPrevTask() => ActionDispatcher.Dispatch(GoToPreviousTaskAction.Create());
		protected void SetCurrentTask(TaskModel task) => ActionDispatcher.Dispatch(SetCurrentTaskAction.Create(task));

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
