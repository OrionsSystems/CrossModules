using System.Collections.Generic;
using Orions.Common;
using Orions.Systems.CrossModules.Components;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class ContentProgressVm : BlazorVm
	{
		public ViewModelProperty<List<ProgressValue>> ExploitedDurationProp { get; set; }
		public ViewModelProperty<List<ProgressValue>> TotalDurationProp { get; set; }
		public ViewModelProperty<List<ProgressValue>> TasksPerformedProp { get; set; }
		public ViewModelProperty<List<ProgressValue>> TasksOutstandingProp { get; set; }
		public ViewModelProperty<List<ProgressValue>> TasksCompletedPerPeriodProp { get; set; }
		
		public ViewModelProperty<List<ProgressValue>> CompletionPercentProp { get; set; }
		public ViewModelProperty<double> CompletionPercentMinValueProp { get; set; }
		public ViewModelProperty<double> CompletionPercentMaxValueProp { get; set; }

		public ViewModelProperty<List<ProgressValue>> SessionsProp { get; set; }
		public ViewModelProperty<long> SessionsMinValueProp { get; set; }
		public ViewModelProperty<long> SessionsMaxValueProp { get; set; }

		public ViewModelProperty<List<ProgressValue>> NewTaggersProp { get; set; }
		public ViewModelProperty<long> NewTaggersMinValueProp { get; set; }
		public ViewModelProperty<long> NewTaggersMaxValueProp { get; set; }

		public ViewModelProperty<List<ProgressValue>> ExploitationSaturationProp { get; set; }
		public ViewModelProperty<double> ExploitationSaturationMinValueProp { get; set; }
		public ViewModelProperty<double> ExploitationSaturationMaxValueProp { get; set; }

		public ViewModelProperty<object[]> CategoriesProp { get; set; }

		public ContentProgressVm()
		{
			ExploitedDurationProp = new ViewModelProperty<List<ProgressValue>>(new List<ProgressValue>());
			TotalDurationProp = new ViewModelProperty<List<ProgressValue>>(new List<ProgressValue>());
			TasksPerformedProp = new ViewModelProperty<List<ProgressValue>>(new List<ProgressValue>());
			TasksOutstandingProp = new ViewModelProperty<List<ProgressValue>>(new List<ProgressValue>());
			TasksCompletedPerPeriodProp = new ViewModelProperty<List<ProgressValue>>(new List<ProgressValue>());

			CompletionPercentProp = new ViewModelProperty<List<ProgressValue>>(new List<ProgressValue>());
			CompletionPercentMinValueProp = new ViewModelProperty<double>(0);
			CompletionPercentMaxValueProp = new ViewModelProperty<double>(5);

			SessionsProp = new ViewModelProperty<List<ProgressValue>>(new List<ProgressValue>());
			SessionsMinValueProp = new ViewModelProperty<long>(0);
			SessionsMaxValueProp = new ViewModelProperty<long>(1000);

			NewTaggersProp = new ViewModelProperty<List<ProgressValue>>(new List<ProgressValue>());
			NewTaggersMinValueProp = new ViewModelProperty<long>(0);
			NewTaggersMaxValueProp = new ViewModelProperty<long>(10);

			ExploitationSaturationProp = new ViewModelProperty<List<ProgressValue>>(new List<ProgressValue>());
			ExploitationSaturationMinValueProp = new ViewModelProperty<double>(0);
			ExploitationSaturationMaxValueProp = new ViewModelProperty<double>(5);

			CategoriesProp = new ViewModelProperty<object[]>(new object[0]);
		}
	}
}
