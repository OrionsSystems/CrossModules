using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Orions.Systems.CrossModules.Components.Model;

using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class FlowDesignerBase<VmType> : BaseBlazorComponent<VmType>, IDisposable
		where VmType : FlowDesignerVm, new()
	{
		protected IDisposable thisReference;

		[Parameter]
		public EventCallback<IFlowDesignData> OnApply { get; set; }

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

		[JSInvokable]
		public async Task OnClickApply()
		{
			//TODO

			await OnApply.InvokeAsync(Vm.DesignData);
		}

		public async Task ToggleCommonMenu()
		{
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.ToggleCommonMenu");
		}

		public async Task HideCommonMenu()
		{
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.HideCommonMenu");
		}

		public async Task ShowCommonMenu()
		{
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.ShowCommonMenu");
		}

		public async Task HideMainMenu()
		{
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.HideMainMenu");
		}

		public async Task ShowMainMenu()
		{
			await JsInterop.InvokeAsync<object>("Orions.FlowDesigner.ShowMainMenu");
		}

		[JSInvokable]
		public async Task OpenPropertyGrid(string id)
		{
			Vm.ShowPropertyGrid(id);

			await ShowCommonMenu();

			StateHasChanged();
		}

		public async Task OnCancelProperty()
		{
			Vm.OnCancelProperty();
			await HideCommonMenu();
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
