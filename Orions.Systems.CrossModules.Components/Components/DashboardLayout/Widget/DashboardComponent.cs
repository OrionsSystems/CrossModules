using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Orions.Node.Common;
using Microsoft.AspNetCore.Components;

namespace Orions.Systems.CrossModules.Components
{
	public interface IDashboardComponent
	{
	}

	public abstract class DashboardComponent : BaseBlazorComponent, IDashboardComponent
	{
		[Parameter]
		public DashboardVm DashboardVm
		{
			get => this.DataContext?.ParentVm as DashboardVm;
			set => this.DataContext.ParentVm = value;
		}

		[Parameter]
		public IDashboardWidget WidgetRaw
		{
			get => ((WidgetVm)this.DataContext)?.Widget;
			set => ((WidgetVm)this.DataContext).Widget = value;
		}


		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => ((WidgetVm)this.DataContext)?.HyperStore;
			set => ((WidgetVm)this.DataContext).HyperStore = value;
		}

		public DashboardComponent()
		{
		}
	}

	public class DashboardComponent<VmType, WidgetType> : DashboardComponent
		where VmType : WidgetVm<WidgetType>, new()
		where WidgetType : class, IDashboardWidget
	{
		/// <summary>
		/// Allows us to assign from non-fully generic values.
		/// </summary>
		[Parameter]
		public VmType Vm
		{
			get => (VmType)this.DataContext;
			set => this.DataContext = (VmType)value;
		}

		[Parameter]
		public WidgetType Widget
		{
			get => this.Vm?.Widget;
			set => this.Vm.Widget = value;
		}

		public DashboardComponent()
		{
		}
	}
}
