﻿using Orions.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(ReportSyncfusionSeriesChartWidget))]
	public class ReportSyncfusionSeriesChartVm : ReportWidgetVm<ReportSyncfusionSeriesChartWidget>
	{
		protected override bool AutoRegisterInDashboard => true;

		public ReportSyncfusionSeriesChartVm()
		{
		}
	}
}
