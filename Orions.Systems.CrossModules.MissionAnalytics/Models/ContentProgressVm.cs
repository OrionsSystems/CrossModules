using System.Collections.Generic;
using Orions.Common;
using Orions.Systems.CrossModules.Blazor;

namespace Orions.Systems.CrossModules.MissionAnalytics
{
	public class ContentProgressVm : BlazorVm
	{
		public ViewModelProperty<List<KeyValueModel>> ExploitedDurationProp { get; set; }
		public ViewModelProperty<List<KeyValueModel>> TotalDurationProp { get; set; }
		public ViewModelProperty<List<KeyValueModel>> TasksPerformedProp { get; set; }
		public ViewModelProperty<List<KeyValueModel>> TasksOutstandingProp { get; set; }
		public ViewModelProperty<List<KeyValueModel>> TasksCompletedPerPeriodProp { get; set; }
		
		public ViewModelProperty<List<KeyValueModel>> CompletionPercentProp { get; set; }
		public ViewModelProperty<double> CompletionPercentMinValueProp { get; set; }
		public ViewModelProperty<double> CompletionPercentMaxValueProp { get; set; }

		public ViewModelProperty<List<KeyValueModel>> SessionsProp { get; set; }
		public ViewModelProperty<long> SessionsMinValueProp { get; set; }
		public ViewModelProperty<long> SessionsMaxValueProp { get; set; }

		public ViewModelProperty<List<KeyValueModel>> NewTaggersProp { get; set; }
		public ViewModelProperty<long> NewTaggersMinValueProp { get; set; }
		public ViewModelProperty<long> NewTaggersMaxValueProp { get; set; }

		public ViewModelProperty<List<KeyValueModel>> ExploitationSaturationProp { get; set; }
		public ViewModelProperty<double> ExploitationSaturationMinValueProp { get; set; }
		public ViewModelProperty<double> ExploitationSaturationMaxValueProp { get; set; }

		public ViewModelProperty<object[]> CategoriesProp { get; set; }

		public ContentProgressVm()
		{
			ExploitedDurationProp = new ViewModelProperty<List<KeyValueModel>>(new List<KeyValueModel>());
			TotalDurationProp = new ViewModelProperty<List<KeyValueModel>>(new List<KeyValueModel>());
			TasksPerformedProp = new ViewModelProperty<List<KeyValueModel>>(new List<KeyValueModel>());
			TasksOutstandingProp = new ViewModelProperty<List<KeyValueModel>>(new List<KeyValueModel>());
			TasksCompletedPerPeriodProp = new ViewModelProperty<List<KeyValueModel>>(new List<KeyValueModel>());

			CompletionPercentProp = new ViewModelProperty<List<KeyValueModel>>(new List<KeyValueModel>());
			CompletionPercentMinValueProp = new ViewModelProperty<double>(0);
			CompletionPercentMaxValueProp = new ViewModelProperty<double>(5);

			SessionsProp = new ViewModelProperty<List<KeyValueModel>>(new List<KeyValueModel>());
			SessionsMinValueProp = new ViewModelProperty<long>(0);
			SessionsMaxValueProp = new ViewModelProperty<long>(1000);

			NewTaggersProp = new ViewModelProperty<List<KeyValueModel>>(new List<KeyValueModel>());
			NewTaggersMinValueProp = new ViewModelProperty<long>(0);
			NewTaggersMaxValueProp = new ViewModelProperty<long>(10);

			ExploitationSaturationProp = new ViewModelProperty<List<KeyValueModel>>(new List<KeyValueModel>());
			ExploitationSaturationMinValueProp = new ViewModelProperty<double>(0);
			ExploitationSaturationMaxValueProp = new ViewModelProperty<double>(5);

			CategoriesProp = new ViewModelProperty<object[]>(new object[0]);
		}
	}
}
