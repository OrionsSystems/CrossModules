using System;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using Orions.Infrastructure.HyperSemantic;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.TagonomyExecution;

namespace Orions.Systems.CrossModules.Desi.Components.VizList
{
	public class VizListComponentBase : BaseComponent
	{
		private ITagonomyExecutionDataStore _store;
		private IDisposable _dataPropertyChangedSub;
		private IDisposable _dataChangedSub;

		[Parameter]
		public string CssClass { get; set; }

		[Parameter]
		public ITagonomyExecutionDataStore Store
		{
			get => _store;
			set => SetProperty(ref _store,
				value,
				() =>
				{
					_dataChangedSub?.Dispose();
					_dataChangedSub = value?.DataChanged.Subscribe(d =>
					{
						UpdateState();
						_dataPropertyChangedSub?.Dispose();
						_dataPropertyChangedSub = d?.GetPropertyChangedObservable().Subscribe(_ => UpdateState());
					});
				});
		}

		[Parameter]
		public IActionDispatcher ActionDispatcher { get; set; }

		protected TagonomyExecutionData Data => _store.Data;

		protected void SelectTagonomyNode(TagonomyNodeModel tagonomyNodeModel) => ActionDispatcher.Dispatch(SelectTagonomyNodeAction.Create(tagonomyNodeModel));
		protected void FinishTagonomyExecution() => ActionDispatcher.Dispatch(FinishTagonomyExecutionAction.Create());
		protected void StepBack() => ActionDispatcher.Dispatch(TagonomyPathAction.Create(TagonomyPathAction.Actions.StepBack));
		protected void GoToStep(TagonomyExecutionStep tagonomyExecutionStep) => ActionDispatcher.Dispatch(GoToStepTagonomyAction.Create(tagonomyExecutionStep));
		protected void FinishCurrentPath() => ActionDispatcher.Dispatch(TagonomyPathAction.Create(TagonomyPathAction.Actions.FinishCurrentPath));
		protected void SkipCurrentPath() => ActionDispatcher.Dispatch(TagonomyPathAction.Create(TagonomyPathAction.Actions.SkipCurrentPath));
		protected void SelectTagonomyProperty(TagonomyPropertyModel tagonomyPropertyModel) => ActionDispatcher.Dispatch(SelectTagonomyPropertyAction.Create(tagonomyPropertyModel));
		protected void FinishInputAction(string value) => ActionDispatcher.Dispatch(FinishTagonomyInputAction.Create(value));
		protected void FinishInputAction(bool value) => ActionDispatcher.Dispatch(FinishTagonomyConfirmationAction.Create(value));

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_dataChangedSub.Dispose();
				_dataPropertyChangedSub?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
