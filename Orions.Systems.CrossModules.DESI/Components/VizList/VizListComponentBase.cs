using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		protected string _componentId = $"vizlist-{Guid.NewGuid().ToString()}";
		private bool _jsInitialized = false;
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

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (!_jsInitialized)
			{
				await JSRuntime.InvokeVoidAsync("window.Orions.Vizlist.init", new object[] { _componentId });
				_jsInitialized = true;
			}

			await base.OnAfterRenderAsync(firstRender);
		}

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
