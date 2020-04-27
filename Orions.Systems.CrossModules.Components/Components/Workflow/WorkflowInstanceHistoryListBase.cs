using Microsoft.AspNetCore.Components;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WorkflowInstanceHistoryListBase : BaseBlazorComponent<WorkflowInstanceHistoryListVm>, IDisposable
	{
		IDisposable thisReference;

		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => Vm.HyperStore;
			set => Vm.HyperStore = value;
		}

		[Parameter]
		public string WorkflowId
		{
			get => Vm.WorkflowId;
			set => Vm.WorkflowId = value;
		}

		public LoaderConfiguration LoaderSetting { get; set; } = new LoaderConfiguration() { Visible = true };

		public WorkflowInstanceHistoryListBase()
		{

		}

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			await Vm.Init();
		}

		void IDisposable.Dispose()
		{
			thisReference?.Dispose();
		}
	}
}
