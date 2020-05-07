using Newtonsoft.Json;

using Orions.Node.Common;
using Orions.Systems.CrossModules.Components.Model;

using System;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class FlowDesignerVm : BlazorVm
	{
		public FlowDesignData DesignData { get; set; } = new FlowDesignData();

		public bool IsShowDesignerSetting { get; set; }
		public bool IsShowProperty { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public PropertyGridVm PropertyGridVm { get; set; } = new PropertyGridVm();

		public FlowDesignerVm()
		{

		}

		public abstract void ShowPropertyGrid(string id);

		public abstract string CreateNode(string json);

		public abstract string DuplicateNode(string id, string json);

		public string GetJesonDesignData()
		{
			return JsonConvert.SerializeObject(DesignData, FlowDesignConverter.Settings);
		}

		public void OnCancelProperty()
		{
			IsShowProperty = false;
			PropertyGridVm.CleanSourceCache();
		}

		protected class FlowMenuItem
		{
			public string Name { get; set; }

			public Type Type { get; set; }

			public string Group { get; set; }

			public string Icon { get; set; }

			public string Color { get; set; }

			public string Html { get; set; }

			public string Readme { get; set; }
		}

	}
}
