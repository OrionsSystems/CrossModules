using System;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using Orions.Infrastructure.HyperSemantic;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.TagonomyExecution;

namespace Orions.Systems.CrossModules.Desi.Components.VizList
{
	public class VizListComponentBase : ComponentBase, IDisposable
	{
		private TagonomyExecutionData _data;
		private IDisposable _userActionStartedSub;

		[Parameter]
		public string CssClass { get; set; }

		[Parameter]
		public TagonomyExecutionData Data
		{
			get => _data;
			set
			{
				_data = value;
				_userActionStartedSub?.Dispose();
				_userActionStartedSub = value?
					.GetPropertyChangedObservable()
					.Where(i => i.EventArgs.PropertyName == nameof(TagonomyExecutionData.PendingUserAction)
					&& i.Source.PendingUserAction != null)
					.Subscribe(_ => InvokeAsync(StateHasChanged));

				StateHasChanged();
			}
		}
		[Parameter]
		public IActionDispatcher ActionDispatcher { get; set; }

		protected void SelectTagonomyNode(TagonomyNodeModel tagonomyNodeModel) => ActionDispatcher.Dispatch(SelectTagonomyNodeAction.Create(tagonomyNodeModel));
		protected void FinishTagonomyExecution() => ActionDispatcher.Dispatch(FinishTagonomyExecutionAction.Create());
		protected void StepBack() => ActionDispatcher.Dispatch(TagonomyPathAction.Create(TagonomyPathAction.Actions.StepBack));
		protected void GoToStep(TagonomyExecutionStep tagonomyExecutionStep) => ActionDispatcher.Dispatch(GoToStepTagonomyAction.Create(tagonomyExecutionStep));
		protected void FinishCurrentPath() => ActionDispatcher.Dispatch(TagonomyPathAction.Create(TagonomyPathAction.Actions.FinishCurrentPath));
		protected void SkipCurrentPath() => ActionDispatcher.Dispatch(TagonomyPathAction.Create(TagonomyPathAction.Actions.SkipCurrentPath));
		protected void SelectTagonomyProperty(TagonomyPropertyModel tagonomyPropertyModel) => ActionDispatcher.Dispatch(SelectTagonomyPropertyAction.Create(tagonomyPropertyModel));
		protected void FinishInputAction(string value) => ActionDispatcher.Dispatch(FinishTagonomyInputAction.Create(value));
		protected void FinishInputAction(bool value) => ActionDispatcher.Dispatch(FinishTagonomyConfirmationAction.Create(value));

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
					_userActionStartedSub?.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~VizListComponentBase()
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
