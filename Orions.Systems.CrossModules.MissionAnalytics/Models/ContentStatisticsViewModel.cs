using System;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class ContentStatisticsViewModel
	{
		public TimeSpan ContentDuration { get; set; }
		public string ContentDurationLabel { get; set; }

		public TimeSpan ExploitingDuration { get; set; } 
		public string ExploitingDurationLabel { get; set; }

		public TimeSpan ExploitedDuration { get; set; } 
		public string ExploitedDurationLabel { get; set; }
		
		public double ExploitedPercentage { get; set; }
		public string ExploitedPercentageLabel { get; set; }

		public TimeSpan TaggerExploitationTime { get; set; }
		public string TaggerExploitationTimeLabel { get; set; }

		public int TaskDone { get; set; }
		
		public int TaskOutstanding { get; set; }
		
		public long Tags { get; set; }

		public long Taggers { get; set; }

		public TimeSpan TotalContentDuration { get; set; }
		public string TotalContentDurationLabel { get; set; }
		
		public TimeSpan TotalExploitingDuration { get; set; }
		public string TotalExploitingDurationLabel { get; set; }
		
		public double TotalExploitedPercentage { get; set; }
		public string TotalExploitedPercentageLabel { get; set; }
		
		public double TodayExploitedPercentage { get; set; }
		public string TodayExploitedPercentageLabel { get; set; }
		
		public long TotalTags { get; set; }
		
		public long TodayTags { get; set; }
		
		public long TotalTaggers { get; set; }
		
		public long TodayTaggers { get; set; }
	}
}
