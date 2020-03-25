using System;
using Microsoft.AspNetCore.Components;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.TaskExploitation;
using Orions.Systems.Desi.Common.Util;

namespace Orions.Systems.CrossModules.Desi.Components.TaskNavigation
{
	public class TaskNavigationWidgetBase: ComponentBase, IDisposable
	{
		private IDisposable _dataTracker;
		private TaskExploitationData _data;

		[Parameter]
		public TaskExploitationData Data
		{
			get => _data;
			set
			{
				_data = value;
				_dataTracker?.Dispose();
				_dataTracker = value?.GetPropertyTracker(() => InvokeAsync(StateHasChanged));
			}
		}

		[Parameter]
		public IActionDispatcher ActionDispatcher { get; set; }

		protected void GoToNextTask() => ActionDispatcher.Dispatch(GoToNextTaskAction.Create());
		protected void GoToPrevTask() => ActionDispatcher.Dispatch(GoToPreviousTaskAction.Create());
		protected void SetCurrentTask(TaskModel task) => ActionDispatcher.Dispatch(SetCurrentTaskAction.Create(task));

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
					_dataTracker?.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~TaskNavigationWidgetBase()
		// {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
