using Orions.Common;
using Orions.Infrastructure.Reporting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardFilterGroupData : NotifyPropertyChanged
	{

		DateTime? _startTime = null;

		/// <summary>
		/// If assigned, the StartTime of our search.
		/// </summary>
		public DateTime? StartTime
		{
			get
			{
				return _startTime;
			}

			set
			{
				SetValue(ref _startTime, value);
			}
		}


		DateTime? _endTime = null;

		/// <summary>
		/// If assigned, the EndTime of our search.
		/// </summary>
		public DateTime? EndTime
		{
			get
			{
				return _endTime;
			}

			set
			{
				SetValue(ref _endTime, value);
			}
		}

		PeriodDefinition _period;

		/// <summary>
		/// If assigned, the step-period we want to apply to our reports.
		/// </summary>
		public PeriodDefinition Period
		{
			get
			{
				return _period;
			}

			set
			{
				SetValue(ref _period, value);
			}
		}

		ReportFilterInstruction.Targets? _filterTarget = null;

		public ReportFilterInstruction.Targets? FilterTarget
		{
			get
			{
				return _filterTarget;
			}

			set
			{
				SetValue(ref _filterTarget, value);
			}
		}

		string[] _filterLabels = null;

		public string[] FilterLabels
		{
			get
			{
				return _filterLabels;
			}

			set
			{
				SetValue(ref _filterLabels, value);
			}
		}


		string _view = "";

		public string View 
		{ 
			get => _view; 
			set => SetValue(ref _view, value); 
		}

		public DashboardFilterGroupData()
		{
		}

		public void Clear()
		{
			this.StartTime = null;
			this.EndTime = null;
			this.Period = null;
			this.FilterTarget = null;
			this.FilterLabels = null;

			this.View = null;
		}
	}
}
