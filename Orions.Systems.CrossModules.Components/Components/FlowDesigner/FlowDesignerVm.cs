using System;
using System.Linq;
using System.Threading.Tasks;
using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class FlowDesignerVm : BlazorVm
	{
		public bool IsShowDesignerSetting { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public PropertyGridVm PropertyGridVm { get; set; } = new PropertyGridVm();

		public FlowDesignerVm()
		{
			
		}

		public void LoadWorkflowDesignerData(string workflowId) 
		{
			throw new NotImplementedException();
		}

		public void LoadWorkflowStatuses(string workflowId, string workflowInstanceId)
		{
			throw new NotImplementedException();
		}


		public void SaveNodeDesignData(string workflowId, string json)
		{
			throw new NotImplementedException();
		}

		public void SaveLocalNodeDesignData(string workflowId, string json)
		{
			throw new NotImplementedException();
		}

		public void CreateLocalNode(string workflowId, string desingComponentJson)
		{
			throw new NotImplementedException();
		}

		public void DuplicateLocalNode(string workflowId, string originalNodeConfigId, string desingComponentJson)
		{
			throw new NotImplementedException();
		}

		private void SaveHyperWorkflowInSession(HyperWorkflow hyperWorkflow)
		{
			throw new NotImplementedException();
		}

		private HyperWorkflow LoadHyperWorkflowFromSession()
		{
			throw new NotImplementedException();
		}

	}
}
