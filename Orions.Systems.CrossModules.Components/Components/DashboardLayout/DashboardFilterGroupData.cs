using Orions.Infrastructure.Reporting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardGroupData
	{
		/// <summary>
		/// If assigned, the StartTime of our search.
		/// </summary>
		public DateTime? StartTime { get; set; }

		/// <summary>
		/// If assigned, the EndTime of our search.
		/// </summary>
		public DateTime? EndTime { get; set; }

		/// <summary>
		/// If assigned, the step-period we want to apply to our reports.
		/// </summary>
		public PeriodDefinition Period { get; set; }

		public ReportFilterInstruction.Targets? FilterTarget { get; set; }

		public string[] FilterLabels { get; set; }

		public DashboardGroupData()
		{
		}

		public void Clear()
		{
			this.StartTime = null;
			this.EndTime = null;
			this.Period = null;
			this.FilterTarget = null;
			this.FilterLabels = null;
		}
	}
}
