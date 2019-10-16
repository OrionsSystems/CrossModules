using System.Collections.Generic;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class ContentProgressViewModel
	{
		public List<KeyValueModel> ExploitedDuration { get; set; }
		public List<KeyValueModel> TotalDuration { get; set; }
		public List<KeyValueModel> TasksPerformed { get; set; }
		public List<KeyValueModel> TasksOutstanding { get; set; }
		public List<KeyValueModel> TasksCompletedPerPeriod { get; set; }
		
		public List<KeyValueModel> CompletionPercent { get; set; }
		public double CompletionPercentMinValue { get; set; }
		public double CompletionPercentMaxValue { get; set; }

		public List<KeyValueModel> Sessions { get; set; }
		public long SessionsMinValue { get; set; }
		public long SessionsMaxValue { get; set; }

		public List<KeyValueModel> NewTaggers { get; set; }
		public long NewTaggersMinValue { get; set; }
		public long NewTaggersMaxValue { get; set; }

		public List<KeyValueModel> ExploitationSaturation { get; set; }
		public double ExploitationSaturationMinValue { get; set; }
		public double ExploitationSaturationMaxValue { get; set; }

		public ContentProgressViewModel()
		{
			ExploitedDuration = new List<KeyValueModel>();
			TotalDuration = new List<KeyValueModel>();
			TasksPerformed = new List<KeyValueModel>();
			TasksOutstanding = new List<KeyValueModel>();
			TasksCompletedPerPeriod = new List<KeyValueModel>();

			CompletionPercent = new List<KeyValueModel>();
			CompletionPercentMinValue = 0;
			CompletionPercentMaxValue = 5;

			Sessions = new List<KeyValueModel>();
			SessionsMinValue = 0;
			SessionsMaxValue = 1000;

			NewTaggers = new List<KeyValueModel>();
			NewTaggersMinValue = 0;
			NewTaggersMaxValue = 10;

			ExploitationSaturation = new List<KeyValueModel>();
			ExploitationSaturationMinValue = 0;
			ExploitationSaturationMaxValue = 5;
		}
	}

	public class KeyValueModel
	{
		public object Key { get; set; }
		public object Value { get; set; }
	}
}
