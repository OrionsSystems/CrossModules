using Newtonsoft.Json;

using Orions.Infrastructure.HyperSemantic;
using Orions.Systems.CrossModules.Components.Model;

using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class TagonomyDesignerVm : FlowDesignerVm
	{
		public string TagonomyId { get; set; }

		public Tagonomy Source { get; set; }

		public Tagonomy Selected { get; set; }

		public TagonomyDesignerVm()
		{

		}

		public virtual async Task Init()
		{
			if (HyperStore == null) return;

			await PopulateData();

			PopulateDesignerData();
		}

		public override void ShowPropertyGrid(string nodeConfigId)
		{
			// load node configuration
			PropertyGridVm.CleanSourceCache();

			//var nodeConfiguration = Source.Nodes.FirstOrDefault(it => it.Id == nodeConfigId);
			//SelectedNode = (HyperWorkflowNodeData)nodeConfiguration.CreateNodeInstance(true);
			//IsShowProperty = true;
		}

		public Task<object> LoadPropertyGridData()
		{
			return Task.FromResult<object>(Selected);
		}

		public override string CreateNode(string desingComponentJson)
		{
			if (string.IsNullOrEmpty(desingComponentJson)) throw new Exception("Missing desing component json");

			FlowDesignComponent nodeConfig = JsonConvert.DeserializeObject<FlowDesignComponent>(desingComponentJson, FlowDesignConverter.Settings);

			//TODO

			var nodeConfigJson = JsonConvert.SerializeObject(nodeConfig, FlowDesignConverter.Settings);

			return nodeConfigJson;
		}

		public override string DuplicateNode(string originalNodeConfigId, string desingComponentJson)
		{
			if (string.IsNullOrEmpty(originalNodeConfigId)) throw new Exception("Missing component id");

			FlowDesignComponent nodeConfig = JsonConvert.DeserializeObject<FlowDesignComponent>(desingComponentJson, FlowDesignConverter.Settings);

			//TODO

			var nodeConfigJson = JsonConvert.SerializeObject(nodeConfig, FlowDesignConverter.Settings);

			return nodeConfigJson;
		}

		private void PopulateDesignerData()
		{
			var design = new FlowDesignData();

			if (Source == null) throw new Exception("Missing source!");

			//TODO

			DesignData = design;

		}


		public void SaveDesignData(string json)
		{
			throw new NotImplementedException();
		}

		private async Task PopulateData()
		{
			//TODO
		}



	}
}
