using System.Collections.Generic;

namespace Orions.Systems.CrossModules.MissionAnalytics.Model
{
	public class ContentProgressViewModel
	{
		public List<KeyValueModel> ExploitedDuration { get; set; }
		public List<KeyValueModel> TotalDuration { get; set; }
		public List<KeyValueModel> TasksPerformed { get; set; }
		public List<KeyValueModel> TasksOutstanding { get; set; }
		public List<KeyValueModel> TasksCompletedPerPeriod { get; set; }
		public List<KeyValueModel> CompletionPercent { get; set; }
		public List<KeyValueModel> Sessions { get; set; }
		public List<KeyValueModel> NewTaggers { get; set; }
		public List<KeyValueModel> ExploitationSaturation { get; set; }

		public ContentProgressViewModel()
		{
			ExploitedDuration = new List<KeyValueModel>();
			TotalDuration = new List<KeyValueModel>();
			TasksPerformed = new List<KeyValueModel>();
			TasksOutstanding = new List<KeyValueModel>();
			TasksCompletedPerPeriod = new List<KeyValueModel>();
			CompletionPercent = new List<KeyValueModel>();
			Sessions = new List<KeyValueModel>();
			NewTaggers = new List<KeyValueModel>();
			ExploitationSaturation = new List<KeyValueModel>();
		}
	}

	public class KeyValueModel
	{
		public object Key { get; set; }
		public object Value { get; set; }
	}
}
