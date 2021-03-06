﻿using Orions.Common;
using Orions.Node.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	public interface IDashboardWidget : IConfig
	{
		string Label { get; set; }

		/// <summary>
		/// Filtering group this widget belongs to.
		/// </summary>
		string FilterGroup { get; set; }

		bool IsVisible { get; set; }
	}

	public interface IReportDashboardWidget : IDashboardWidget
	{
		public bool AppendPercentageSign { get; set; }
	}
}
