using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Components.Model;

namespace Orions.Systems.CrossModules.Components
{
	public class FlowDesignerVm : BlazorVm
	{
		public HyperWorkflow Source { get; set; }

		public FlowDesignData DesignData { get; private set; } 	 = new FlowDesignData();

		/// <summary>
		/// Statuses for this view model.
		/// </summary>
		public HyperWorkflowStatus[] WorkflowStatuses { get; set; }


		public bool IsShowDesignerSetting { get; set; }
		public bool IsShowProperty { get; set; }

		public HyperWorkflowNodeData SelectedNode { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public PropertyGridVm PropertyGridVm { get; set; } = new PropertyGridVm();

		public FlowDesignerVm()
		{
			
		}

		public virtual async Task Init() 
		{ 
			var workflowId = "f4a66ad3-b538-4bcd-8e09-739e4b37e016";
			var workflowInstanceId = "";

			await PopulateWorkflow(workflowId);
			await PopulateWorkflowStatuses(workflowInstanceId);

			PopulateDesignerData();
		}

		public string GetJesonDesignData() {
			return JsonConvert.SerializeObject(DesignData, FlowDesignConverter.Settings);
		}

		public void ShowPropertyGrid(string nodeConfigId) 
		{
			// load node configuration

			var nodeConfiguration = Source.Nodes.FirstOrDefault(it => it.Id == nodeConfigId);

			SelectedNode = (HyperWorkflowNodeData)nodeConfiguration.CreateNodeInstance(true);

			IsShowProperty = true;
		}

		public Task<object> LoadPropertyGridData()
		{
			return Task.FromResult<object>(SelectedNode);
		}

		public void OnCancelProperty() {
			IsShowProperty = false;
			PropertyGridVm.CleanSourceCache();
		}

		private void PopulateDesignerData() 
		{
			var design = new FlowDesignData();

			var types = GetHyperWorkflowNodeDataType();

			foreach (var nodeType in types)
			{
				var nodeName = nodeType.Name;
				var nodeConfigId = String.Copy(nodeName);

				nodeName = RemovePrefix(nodeName);

				var nodeNameFull = nodeType.FullName;

				var node = new NodeConfiguration(nodeType);
				var inputCount = node.InputCount;
				var outputCount = node.OutputCount;
				var group = node.Group;
				var color = UniColorToHex(node.Color);

				var designNodeConfiguration = new FlowDesignNodeConfiguration()
				{
					Id = nodeConfigId.ToLower(),
					TypeFull = nodeNameFull,
					Title = nodeName,
					Input = inputCount,
					Output = outputCount,
				};

				if (!string.IsNullOrEmpty(color)) designNodeConfiguration.Color = color;
				if (!string.IsNullOrEmpty(group)) designNodeConfiguration.Group = group;

				design.NodeConfigurations.Add(designNodeConfiguration);
			}

			// TODO : Save data in session !!!

			design.FlowName = Source.Name;
			design.FlowId = Source.Id;

			foreach (NodeConfiguration nodeConfig in Source.Nodes)
			{
				//add node
				var x = nodeConfig.GUIPosition.X;
				var y = nodeConfig.GUIPosition.Y;

				var color = UniColorToHex(nodeConfig.Color);
				var group = nodeConfig.Group;

				var component = new FlowDesignComponent()
				{
					X = Convert.ToInt64(x),
					Y = Convert.ToInt64(y),
					Name = nodeConfig.NodeType.Name.ToLower(),
					Id = nodeConfig.Id,
					Type = nodeConfig.NodeType.TypeName,
					State = new FlowState()
					{
						Text = string.IsNullOrWhiteSpace(nodeConfig.Name) ? nodeConfig.NodeType.Name : nodeConfig.Name,
						Color = color
					},
					Group = group
				};

				if (!string.IsNullOrEmpty(color)) component.State.Color = color;
				if (!string.IsNullOrEmpty(group)) component.Group = group;

				//connections
				foreach (var connection in nodeConfig.OutputConnections.Where(it => it != null))
				{
					var source = Source.GetNodeConfiguration(connection.SourceId);
					var target = Source.GetNodeConfiguration(connection.TargetId);

					var sourceConnectionId = connection.SourceId;
					var targetConnectionId = connection.TargetId;

					var con = new FlowConnection()
					{
						Id = targetConnectionId,
						Index = "0"
					};

					component.Connections.Add(con);
				}

				design.Components.Add(component);
			}

			DesignData =  design;

		}

		public void LoadWorkflowStatuses(string workflowId, string workflowInstanceId)
		{
			throw new NotImplementedException();
		}


		public void SaveNodeDesignData(string workflowId, string json)
		{
			throw new NotImplementedException();
		}

		private async Task PopulateWorkflow(string workflowId) 
		{
			if (HyperStore == null) return;

			var documentId = HyperDocumentId.Create<HyperWorkflow>(workflowId);
			var args = new RetrieveHyperDocumentArgs(documentId);
			var doc = await HyperStore.ExecuteAsync(args);

			if (args.ExecutionResult.IsNotSuccess)
				return;

			Source =  doc?.GetPayload<HyperWorkflow>();
		}

		private async Task PopulateWorkflowStatuses(string workflowInstanceId) 
		{

			if (HyperStore == null) return;
			if (string.IsNullOrWhiteSpace(workflowInstanceId)) return;

			try 
			{
				var args = new RetrieveHyperWorkflowsStatusesArgs() { WorkflowInstancesIds = new string[] { workflowInstanceId } };

				WorkflowStatuses = await HyperStore.ExecuteAsyncThrows(args);

			} catch(Exception ex) {
				Console.WriteLine(ex.Message);
			}
			// Pull all working statuses.
			
		}

		private List<Type> GetHyperWorkflowNodeDataType()
		{
			List<Type> types = ReflectionHelper.Instance.GatherTypeChildrenTypesFromAssemblies(typeof(HyperWorkflowNodeData),
				 ReflectionHelper.Instance.AllAssemblies);

			types = types.Where(it => it.IsAbstract == false && it.GetCustomAttributes(true).Any(it2 => it2 is ObsoleteAttribute) == false).ToList();

			return types;
		}


		private void SaveHyperWorkflowInSession(HyperWorkflow hyperWorkflow)
		{
			throw new NotImplementedException();
		}

		private HyperWorkflow LoadHyperWorkflowFromSession()
		{
			throw new NotImplementedException();
		}

		private string[] _remStr = new string[] { "HyperWorkflowNodeData", "WorkflowNodeData" };
		private string RemovePrefix(string value) {
			foreach (var sciptEndPrefix in _remStr)
			{
				if (value.EndsWith(sciptEndPrefix))
				{
					value = value.Substring(0, value.LastIndexOf(sciptEndPrefix));
					return value;
				}
			}

			return value;
		}

		private string UniColorToHex(UniColor color)
		{
			if (color.IsEmpty) return "#7d7d7d";

			return string.Format("#{0:X2}{1:X2}{2:X2}",
						color.R,
						color.G,
						color.B);
		}

	}
}
