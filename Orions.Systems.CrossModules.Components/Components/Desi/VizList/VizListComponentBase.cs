using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Infrastructure.HyperSemantic;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;
using Orions.Systems.CrossModules.Components.Desi.Services;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.TagonomyExecution;

namespace Orions.Systems.CrossModules.Components.Desi.VizList
{
	public class VizListComponentBase : BaseComponent
	{
		private const int UpdateStateTickRateMilliseconds = 30;
		private IDisposable _dataPropertyChangedSub;
		private IDisposable _dataChangedSub;

		[Parameter]
		public string CssClass { get; set; }

		[Inject]
		public ITagonomyExecutionDataStore Store { get; set; }

		[Inject]
		public IActionDispatcher ActionDispatcher { get; set; }

		[Inject]
		public IPopupService PopupService { get; set; }

		[Parameter]
		public Action VizListRendered { get; set; }

		protected string _componentId = $"vizlist-{Guid.NewGuid().ToString()}";
		protected TagonomyExecutionData Data => Store.Data;

		protected void SelectTagonomyNode(TagonomyNodeModel tagonomyNodeModel) => ActionDispatcher.Dispatch(SelectTagonomyNodeAction.Create(tagonomyNodeModel));
		protected void FinishTagonomyExecution() => ActionDispatcher.Dispatch(FinishTagonomyExecutionAction.Create());
		protected void StepBack() => ActionDispatcher.Dispatch(TagonomyPathAction.Create(TagonomyPathAction.Actions.StepBack));
		protected void GoToStep(TagonomyExecutionStep tagonomyExecutionStep) => ActionDispatcher.Dispatch(GoToStepTagonomyAction.Create(tagonomyExecutionStep));
		protected void FinishCurrentPath() => ActionDispatcher.Dispatch(TagonomyPathAction.Create(TagonomyPathAction.Actions.FinishCurrentPath));
		protected void SkipCurrentPath() => ActionDispatcher.Dispatch(TagonomyPathAction.Create(TagonomyPathAction.Actions.SkipCurrentPath));
		protected void SelectTagonomyProperty(TagonomyPropertyModel tagonomyPropertyModel) => ActionDispatcher.Dispatch(SelectTagonomyPropertyAction.Create(tagonomyPropertyModel));
		protected void FinishInputAction(string value) => ActionDispatcher.Dispatch(FinishTagonomyInputAction.Create(value));
		protected void FinishInputAction(bool value) => ActionDispatcher.Dispatch(FinishTagonomyConfirmationAction.Create(value));
		protected void CancelExecution() => ActionDispatcher.Dispatch(CancelTagonomyExecutionAction.Create());

		protected override async Task OnAfterRenderAsyncSafe(bool firstRender)
		{
			if(Data?.TagonomyNodes?.Any() != null && PopupService != null)
			{
				var nodesWithUIPopper = Data.TagonomyNodes.Where(n => n.GuiNodeElement != null).ToList();
				foreach(var node in nodesWithUIPopper)
				{
					var referenceElementId = GetTagonomyNodeButtonId(node);
					PopupService.RegisterTagonomyNodePopper(node, referenceElementId);
				}
			}


			await this.JSRuntime.InvokeVoidAsync("Orions.Vizlist.init", _componentId);
			this.VizListRendered?.Invoke();
		}

		protected override void OnInitializedSafe()
		{
			base.OnInitializedSafe();

			_dataChangedSub = Store.DataChanged.Subscribe(d =>
			{
				UpdateState();
				_dataPropertyChangedSub?.Dispose();
				_dataPropertyChangedSub = d?
					.GetPropertyChangedObservable()
					.Sample(TimeSpan.FromMilliseconds(UpdateStateTickRateMilliseconds))
					.Subscribe(_ => UpdateState());
			});
		}

		protected string GetTagonomyNodeButtonId(TagonomyNodeModel tagonomyNode)
		{
			return $"node-id-{tagonomyNode.Id}";
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_dataChangedSub?.Dispose();
				_dataPropertyChangedSub?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
