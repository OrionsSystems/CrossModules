using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Orions.Node.Common;
using Microsoft.AspNetCore.Components;

namespace Orions.Systems.CrossModules.Components
{
	public interface IWidgetComponent
	{
	}

	public class WidgetComponent<VmType, WidgetType> : BaseBlazorComponent<VmType>, IWidgetComponent
		where VmType : WidgetVm<WidgetType>, new()
		where WidgetType : class, IDashboardWidget
	{

		[ParameterAttribute]
		public WidgetType Widget
		{
			get => this.DataContext?.Widget;
			set => this.DataContext.Widget = value;
		}

		[ParameterAttribute]
		public IHyperArgsSink HyperStore
		{
			get => this.DataContext?.HyperStore;
			set => this.DataContext.HyperStore = value;
		}

		public WidgetComponent()
		{
		}
	}
}
