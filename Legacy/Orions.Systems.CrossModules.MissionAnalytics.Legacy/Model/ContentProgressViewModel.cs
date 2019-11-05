using System;

namespace Orions.Systems.CrossModules.MissionAnalytics.Model
{
	public class ContentProgressViewModel
	{
		private static int _roundDecimalFactor = 2;

		public string Content { get; set; }

		public DateTime Date { get; set; }

		public double ExploitedDuration { get; set; } //Return exploited duration in hours
		public double ExploitedDurationRound
		{
			get { return Math.Round(ExploitedDuration, _roundDecimalFactor, MidpointRounding.AwayFromZero); }
		}
		public double TotalDuration { get; set; } //Return total duration in hours
		public double TotalDurationRound
		{
			get { return Math.Round(TotalDuration, _roundDecimalFactor, MidpointRounding.AwayFromZero); }
		}
		public int TasksPerformed { get; set; }
		public int TasksOutstanding { get; set; }
		public int TasksCompletedPerPeriod { get; set; }
		public double CompletionPercent { get; set; }
		public double CompletionPercentRound
		{
			get { return Math.Round(CompletionPercent, _roundDecimalFactor, MidpointRounding.AwayFromZero); }
		}
		public long Sessions { get; set; }
		public long NewTaggers { get; set; }
		public double ExploitationSaturation { get; set; }
		public double ExploitationSaturationRound
		{
			get { return Math.Round(ExploitationSaturation, _roundDecimalFactor, MidpointRounding.AwayFromZero); }
		}
	}
}
