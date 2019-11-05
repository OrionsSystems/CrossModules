namespace Orions.Systems.CrossModules.MissionAnalytics.Model
{
	public class WorkflowConfigNodeViewModel
	{
		public string Id { get; set; }

		public string MissionInstanceId { get; set; }

		public string Name { get; set; }

		public string Label
		{
			get
			{
				return $"{Name} - {Id}";
			}
		}
	}
}
