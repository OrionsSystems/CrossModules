﻿using Microsoft.AspNetCore.Components;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WorkflowListBase : BaseBlazorComponent<WorkflowListVm>, IDisposable
	{
		IDisposable thisReference;

		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => Vm.HyperStore;
			set => Vm.HyperStore = value;
		}

		[Parameter]
		public EventCallback<HyperWorkflow> OnManageWorkflow
		{
			get => Vm.OnManageWorkflow;
			set => Vm.OnManageWorkflow = value;
		}

		[Parameter]
		public EventCallback<HyperWorkflow> OnOpenWorkflowInstances
		{
			get => Vm.OnOpenWorkflowInstances;
			set => Vm.OnOpenWorkflowInstances = value;
		}

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

		public LoaderConfiguration LoaderSetting { get; set; } = new LoaderConfiguration() { Visible = true };

		public WorkflowListBase()
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
