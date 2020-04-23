using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Orions.Node.Common;

using System;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.Components.Model;

namespace Orions.Systems.CrossModules.Components
{
	public class FlowDesignerBase : BaseBlazorComponent<FlowDesignerVm>, IDisposable
	{
		IDisposable thisReference;

		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => Vm.HyperStore;
			set => Vm.HyperStore = value;
		}

		[Parameter]
		public string WorkflowId { get => Vm.WorkflowId; set => Vm.WorkflowId = value; }

		[Parameter]
		public string WorkflowInstanceId { get => Vm.WorkflowInstanceId; set => Vm.WorkflowInstanceId = value; }

		[Parameter]
		public EventCallback<IFlowDesignData> OnApply { get; set; }

		PropertyGrid _propGrid;
		PropertyGrid propGrid
		{
			get => _propGrid;
			set
			{
				_propGrid = value;
				Vm.PropertyGridVm = value.Vm;
			}
		}

		public FlowDesignerBase()
		{

		}

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

		}

		protected override async Task OnFirstAfterRenderAsync()
		{
			await Vm.Init();

			var jsonData = Vm.GetJesonDesignData();

			thisReference = DotNetObjectReference.Create(this);

			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.Init", new object[] { thisReference, jsonData });

		}

		public async Task ToggleCommonMenu() 
		{
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.ToggleCommonMenu");
		}

		[JSInvokable]
		public async  Task OnClickApply()
		{
			//TODO

			await OnApply.InvokeAsync(Vm.DesignData);
		}

		[JSInvokable]
		public void OpenPropertyGrid(string id)
		{
			Vm.ShowPropertyGrid(id);

			StateHasChanged();
		}

		[JSInvokable]
		public string CreateNode(string desingComponentJson)
		{
			return Vm.CreateNode(desingComponentJson);
		}

		[JSInvokable]
		public string DuplicateNode(string originalNodeConfigId, string desingComponentJson)
		{
			return Vm.DuplicateNode(originalNodeConfigId, desingComponentJson);
		}

		public async Task ToggleMainMenu() 
		{
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.ToggleMainMenu");
		}

		public async Task CopyElement()
		{
			thisReference = DotNetObjectReference.Create(this);
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.Copy", new object[] { thisReference });
		}

		public async Task PasteElement()
		{
			thisReference = DotNetObjectReference.Create(this);
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.Paste", new object[] { thisReference });
		}

		public async Task SettingsElement() 
		{
			thisReference = DotNetObjectReference.Create(this);
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.Settings", new object[] { thisReference });
		}

		public async Task DuplicateElement()
		{
			thisReference = DotNetObjectReference.Create(this);
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.Duplicate", new object[] { thisReference });
		}

		public async Task RemoveElement()
		{
			thisReference = DotNetObjectReference.Create(this);
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.Remove", new object[] { thisReference });
		}

		public async Task ZoomIn()
		{
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.ZoomIn");
		}

		public async Task ZoomReset() 
		{
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.ZoomReset");
		}

		public async Task ZoomOut() 
		{
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.ZoomOut");
		}

		void IDisposable.Dispose()
		{
			thisReference?.Dispose();
		}
	}
}
