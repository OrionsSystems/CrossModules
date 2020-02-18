using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Reporting;
using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	/// <summary>
	/// Optional generic class helps assign a specialized type for the Widget class.
	/// </summary>
	public class WidgetVm<WidgetType> : WidgetVm
		where WidgetType : IDashboardWidget
	{
		public new WidgetType Widget
		{
			get => (WidgetType)base.Widget;
			set => base.Widget = value;
		}

		public WidgetVm()
		{
		}
	}

	public class WidgetVm : BlazorVm
	{
		IHyperArgsSink _hyperStore = null;
		public IHyperArgsSink HyperStore
		{
			get
			{
				return _hyperStore;
			}

			set
			{
				if (value != null)
					_hyperStore = value;
			}
		}

		protected virtual bool AutoRegisterInDashboard => false;

		/// <summary>
		/// The Vm of the dashboard this widget belongs to.
		/// </summary>
		public DashboardVm DashboardVm => this.ParentVm as DashboardVm;

		public IDashboardWidget Widget { get; set; }

		public WidgetVm()
		{
		}

		//protected override void OnSetParentVm(BaseVm parentVm)
		//{
		//	base.OnSetParentVm(parentVm);
		//	//if (this.AutoRegisterInDashboard && parentVm is DashboardVm dashboardVm)
		//	//{
		//	//	dashboardVm.TryAddWidgetVm(this);
		//	//}
		//}

		public virtual Task HandleFiltersChangedAsync()
		{
			return Task.CompletedTask;
		}

		

	}
}
