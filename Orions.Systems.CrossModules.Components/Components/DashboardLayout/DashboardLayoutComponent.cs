using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardLayoutComponent : BaseBlazorComponent<DashboardVm>
	{
		protected override bool AutoCreateVm => false;

		public DashboardLayoutComponent()
		{
			var vm = new DashboardVm();
			this.DataContext = vm;
		}
	}
}
