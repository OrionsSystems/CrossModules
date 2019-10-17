using System;
using Orions.Common;
using Orions.Systems.CrossModules.Blazor;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class ContentStatsVm : BlazorVm
	{
		public ViewModelProperty<TimeSpan> ContentDurationProp { get; set; }
		public ViewModelProperty<string> ContentDurationLabelProp { get; set; }

		public ViewModelProperty<TimeSpan> ExploitingDurationProp { get; set; }
		public ViewModelProperty<string> ExploitingDurationLabelProp { get; set; }

		public ViewModelProperty<TimeSpan> ExploitedDurationProp { get; set; }
		public ViewModelProperty<string> ExploitedDurationLabelProp { get; set; }

		public ViewModelProperty<double> ExploitedPercentageProp { get; set; }
		public ViewModelProperty<string> ExploitedPercentageLabelProp { get; set; }

		public ViewModelProperty<TimeSpan> TaggerExploitationTimeProp { get; set; }
		public ViewModelProperty<string> TaggerExploitationTimeLabelProp { get; set; }

		public ViewModelProperty<int> TaskDoneProp { get; set; }
		public ViewModelProperty<int> TaskOutstandingProp { get; set; }

		public ViewModelProperty<TimeSpan> TotalContentDurationProp { get; set; }
		public ViewModelProperty<string> TotalContentDurationLabelProp { get; set; }

		public ViewModelProperty<TimeSpan> TotalExploitingDurationProp { get; set; }
		public ViewModelProperty<string> TotalExploitingDurationLabelProp { get; set; }

		public ViewModelProperty<double> TotalExploitedPercentageProp { get; set; }
		public ViewModelProperty<string> TotalExploitedPercentageLabelProp { get; set; }

		public ViewModelProperty<double> TodayExploitedPercentageProp { get; set; }
		public ViewModelProperty<string> TodayExploitedPercentageLabelProp { get; set; }

		public ViewModelProperty<long> TagsProp { get; set; }
		public ViewModelProperty<long> TotalTagsProp { get; set; }
		public ViewModelProperty<long> TodayTagsProp { get; set; }

		public ViewModelProperty<long> TaggersProp { get; set; }
		public ViewModelProperty<long> TotalTaggersProp { get; set; }
		public ViewModelProperty<long> TodayTaggersProp { get; set; }

		public ContentStatsVm()
		{
			ContentDurationProp = new ViewModelProperty<TimeSpan>();
			ContentDurationLabelProp = new ViewModelProperty<string>();

			ExploitingDurationProp = new ViewModelProperty<TimeSpan>();
			ExploitingDurationLabelProp = new ViewModelProperty<string>();

			ExploitedDurationProp = new ViewModelProperty<TimeSpan>();
			ExploitedDurationLabelProp = new ViewModelProperty<string>();

			ExploitedPercentageProp = new ViewModelProperty<double>();
			ExploitedPercentageLabelProp = new ViewModelProperty<string>();

			TaggerExploitationTimeProp = new ViewModelProperty<TimeSpan>();
			TaggerExploitationTimeLabelProp = new ViewModelProperty<string>();

			TaskDoneProp = new ViewModelProperty<int>();
			TaskOutstandingProp = new ViewModelProperty<int>();

			TotalContentDurationProp = new ViewModelProperty<TimeSpan>();
			TotalContentDurationLabelProp = new ViewModelProperty<string>();

			TotalExploitingDurationProp = new ViewModelProperty<TimeSpan>();
			TotalExploitingDurationLabelProp = new ViewModelProperty<string>();

			TotalExploitedPercentageProp = new ViewModelProperty<double>();
			TotalExploitedPercentageLabelProp = new ViewModelProperty<string>();

			TodayExploitedPercentageProp = new ViewModelProperty<double>();
			TodayExploitedPercentageLabelProp = new ViewModelProperty<string>();

			TagsProp = new ViewModelProperty<long>();
			TotalTagsProp = new ViewModelProperty<long>();
			TodayTagsProp = new ViewModelProperty<long>();

			TaggersProp = new ViewModelProperty<long>();
			TotalTaggersProp = new ViewModelProperty<long>();
			TodayTaggersProp = new ViewModelProperty<long>();
		}
	}
}
