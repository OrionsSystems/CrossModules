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

	public class DashboardComponent<VmType, WidgetType> : BaseBlazorComponent<VmType>, IDashboardComponent
		where VmType : WidgetVm<WidgetType>, new()
		where WidgetType : class, IDashboardWidget
	{
		[Parameter]
		public DashboardVm DashboardVm
		{
			get => this.DataContext?.ParentVm as DashboardVm;
			set => this.DataContext.ParentVm = value;
		}

		[Parameter]
		public WidgetType Widget
		{
			get => this.DataContext?.Widget;
			set => this.DataContext.Widget = value;
		}

		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => this.DataContext?.HyperStore;
			set => this.DataContext.HyperStore = value;
		}

		public DashboardComponent()
		{
		}
	}
}
