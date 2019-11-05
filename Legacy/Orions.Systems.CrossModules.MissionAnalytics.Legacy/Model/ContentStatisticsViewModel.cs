using System;

namespace Orions.Systems.CrossModules.MissionAnalytics.Model
{
	public class ContentStatisticsViewModel
	{

		private static string _dateTemplate = "{0:hh\\:mm\\:ss}";
		private static int _roundDecimalFactor = 2;

		public TimeSpan ContentDuration { get; set; }
		public string ContentDurationLabel
		{
			get
			{
				return string.Format(_dateTemplate, ContentDuration);
			}
		}

		public TimeSpan ExploitingDuration { get; set; } //Exploiting Content
		public string ExploitingDurationLabel
		{
			get
			{
				return string.Format(_dateTemplate, ExploitingDuration);
			}
		}

		public TimeSpan ExploitedDuration { get; set; } //Exploited Content
		public string ExploitedDurationLabel
		{
			get
			{
				return string.Format(_dateTemplate, ExploitedDuration);
			}
		}

		public TimeSpan TaggerExploitationTime { get; set; }
		public string TaggerExploitationTimeLabel
		{
			get
			{
				return string.Format(_dateTemplate, TaggerExploitationTime);
			}
		}

		public int TaskDone { get; set; }
		public int TaskOutstanding { get; set; }
		public double ExploitedPercentage { get; set; }
		public double ExploitedPercentageRound
		{
			get { return Math.Round(ExploitedPercentage, _roundDecimalFactor, MidpointRounding.AwayFromZero); }
		}

		public long Tags { get; set; }

		public long Taggers { get; set; }

		public TimeSpan TotalContentDuration { get; set; }
		public string TotalContentDurationLabel { get { return string.Format(_dateTemplate, TotalContentDuration); }}
		public TimeSpan TotalExploitingDuration { get; set; }
		public string TotalExploitingDurationLabel { get { return string.Format(_dateTemplate, TotalExploitingDuration); } }
		public double TotalExploitedPercentage { get; set; }
		public double TotalExploitedPercentageRound
		{
			get { return Math.Round(TotalExploitedPercentage, _roundDecimalFactor, MidpointRounding.AwayFromZero); }
		}
		public double TodayExploitedPercentage { get; set; }
		public double TodayExploitedPercentageRound
		{
			get { return Math.Round(TodayExploitedPercentage, _roundDecimalFactor, MidpointRounding.AwayFromZero); }
		}
		public long TotalTags { get; set; }
		public long TodayTags { get; set; }
		public long TotalTaggers { get; set; }
		public long TodayTaggers { get; set; }
	}
}
