using Newtonsoft.Json;

using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Components.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class FlowDesignerVm : BlazorVm
	{
		public HyperWorkflow Source { get; set; }

		public string WorkflowId { get; set; }
		public string WorkflowInstanceId { get; set; }

		public FlowDesignData DesignData { get; private set; } = new FlowDesignData();

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
			if (HyperStore == null) return;

			await PopulateWorkflow(WorkflowId);

			await PopulateWorkflowStatuses(WorkflowInstanceId);

			PopulateDesignerData();
		}

		public string GetJesonDesignData()
		{
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

		public void OnCancelProperty()
		{
			IsShowProperty = false;
			PropertyGridVm.CleanSourceCache();
		}

		public string CreateNode(string desingComponentJson)
		{
			if (string.IsNullOrEmpty(desingComponentJson)) throw new Exception("Missing desing component json");

			FlowDesignComponent nodeConfig = JsonConvert.DeserializeObject<FlowDesignComponent>(desingComponentJson, FlowDesignConverter.Settings);


			var types = GetHyperWorkflowNodeDataTypes();
			var type = types.FirstOrDefault(it => it.FullName == nodeConfig.Type);

			var node = new NodeConfiguration(type)
			{
				AllowMultiOutputPortConnections = true
			};

			nodeConfig.Id = node.Id;

			var nodeName = nodeConfig.State?.Text;
			if (nodeName != null) node.Name = nodeName;

			var nodeColor = nodeConfig.State?.Color;
			if (!string.IsNullOrEmpty(nodeColor))
			{
				var colorR = UniColorFromHex(nodeColor);
				node.Color = colorR;
			}

			var nodeGroup = nodeConfig.Group;
			if (nodeGroup != null) node.Group = nodeGroup;

			node.GUIPosition = new UniPoint2f(nodeConfig.X, nodeConfig.Y);

			Source.AddNode(node);

			var nodeConfigJson = JsonConvert.SerializeObject(nodeConfig, FlowDesignConverter.Settings);

			return nodeConfigJson;

		}

		public string DuplicateNode(string originalNodeConfigId, string desingComponentJson)
		{

			if (string.IsNullOrEmpty(originalNodeConfigId)) throw new Exception("Missing node component id");

			var oldNodeConfiguration = Source.Nodes?.FirstOrDefault(it => it.Id == originalNodeConfigId);

			if (oldNodeConfiguration == null) throw new Exception("Missing node configuration");

			var hyperWorkflowHyperNodeData = (HyperWorkflowNodeData)oldNodeConfiguration.CreateNodeInstance(true);

			FlowDesignComponent nodeConfig = JsonConvert.DeserializeObject<FlowDesignComponent>(desingComponentJson, FlowDesignConverter.Settings);

			var types = GetHyperWorkflowNodeDataTypes();
			var type = types.FirstOrDefault(it => it.FullName == nodeConfig.Type);

			if (type == null) throw new ApplicationException("Missing node type");

			var node = new NodeConfiguration(type)
			{
				AllowMultiOutputPortConnections = true
			};

			node.CopySettingsFromNode(hyperWorkflowHyperNodeData);

			nodeConfig.Id = node.Id;

			var nodeName = nodeConfig.State.Text;
			if (nodeName != null) node.Name = nodeName;

			var nodeColor = nodeConfig.State.Color;
			if (!string.IsNullOrEmpty(nodeColor))
			{
				var colorR = UniColorFromHex(nodeColor);
				node.Color = colorR;
			}

			var nodeGroup = nodeConfig.Group;
			if (nodeGroup != null) node.Group = nodeGroup;

			node.GUIPosition = new UniPoint2f(nodeConfig.X, nodeConfig.Y);

			Source.AddNode(node);

			var nodeConfigJson = JsonConvert.SerializeObject(nodeConfig, FlowDesignConverter.Settings);

			return nodeConfigJson;
		}

		private void PopulateDesignerData()
		{
			var design = new FlowDesignData();

			var nodeDataGroupTypes = GroupingNodeDataTypes();

			nodeDataGroupTypes = MappingMenuColorAndIcons(nodeDataGroupTypes);

			foreach (var nodeGroupType in nodeDataGroupTypes)
			{
				var nodeType = nodeGroupType.Type;

				var nodeName = nodeGroupType.Name;
				var nodeConfigId = nodeType.Name;

				var nodeNameFull = nodeType.FullName;

				var node = new NodeConfiguration(nodeType);
				var inputCount = node.InputCount;
				var outputCount = node.OutputCount;
				var color = UniColorToHex(node.Color);

				var designNodeConfiguration = new FlowDesignNodeConfiguration()
				{
					Id = nodeConfigId.ToLower(),
					TypeFull = nodeNameFull,
					Title = nodeName,
					Input = inputCount,
					Output = outputCount,
					Group = nodeGroupType.Group,
					Color = nodeGroupType.Color,
					Icon = nodeGroupType.Icon
				};

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
					Type = nodeConfig.NodeType.AsType().FullName,
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

			DesignData = design;

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

			Source = doc?.GetPayload<HyperWorkflow>();
		}

		private async Task PopulateWorkflowStatuses(string workflowInstanceId)
		{

			if (HyperStore == null) return;
			if (string.IsNullOrWhiteSpace(workflowInstanceId)) return;

			try
			{
				var args = new RetrieveHyperWorkflowsStatusesArgs() { WorkflowInstancesIds = new string[] { workflowInstanceId } };

				WorkflowStatuses = await HyperStore.ExecuteAsyncThrows(args);

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			// Pull all working statuses.

		}

		private List<FlowMenuItem> GroupingNodeDataTypes()
		{
			var result = new List<FlowMenuItem>();

			var types = GetHyperWorkflowNodeDataTypes();

			var decoratedNodeTypes = types.Where(t => t.GetCustomAttributes(typeof(GroupAttribute), true).Length > 0).ToList();

			var otherNodeTypes = types.Where(t => t.GetCustomAttributes(typeof(GroupAttribute), true).Length == 0).ToList();

			foreach (var item in Enum.GetValues(typeof(GroupAttribute.SystemGroups)))
			{
				var enumName = Enum.GetName(typeof(GroupAttribute.SystemGroups), item);

				var selectedNodeTypes = decoratedNodeTypes.Where(t => t.GetCustomAttribute<GroupAttribute>().Name == enumName).ToArray();

				foreach (var type in selectedNodeTypes)
				{
					var name = GetComponentNameForContextMenu(type);

					result.Add(new FlowMenuItem { Group = enumName, Name = name, Type = type });

				}

				if (enumName == GroupAttribute.SystemGroups.Other.ToString())
				{
					foreach (var otherConfigType in otherNodeTypes)
					{
						var name = GetComponentNameForContextMenu(otherConfigType);

						result.Add(new FlowMenuItem { Group = enumName, Name = name, Type = otherConfigType });
					}
				}
			}

			return result;
		}

		private List<FlowMenuItem> MappingMenuColorAndIcons(List<FlowMenuItem> data)
		{
			foreach (var item in data)
			{
				var tempItem = _templateMenuItems.Where(it => it.Group == item.Group).FirstOrDefault();
				if (tempItem != null) {
					item.Icon = tempItem.Icon;
					item.Color = tempItem.Color;
				}

			}

			return data;
		}

		private string GetComponentNameForContextMenu(Type configType)
		{
			var truncatedName = configType.Name.Replace("HyperWorkflowNodeData", string.Empty)
														 .Replace("WorkflowNodeData", string.Empty) ?? "";

			return StringHelper.SplitCamelCase(truncatedName);
		}

		private List<Type> GetHyperWorkflowNodeDataTypes()
		{
			List<Type> types = ReflectionHelper.Instance.GatherTypeChildrenTypesFromAssemblies(typeof(HyperWorkflowNodeData), ReflectionHelper.Instance.AllAssemblies);

			types = types.Where(it => it.IsAbstract == false && it.GetCustomAttributes(true).Any(it2 => it2 is ObsoleteAttribute) == false).OrderBy(it => it.Name).ToList();

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
		private string RemovePrefix(string value)
		{
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

		private UniColor UniColorFromHex(string hex)
		{
			if (string.IsNullOrWhiteSpace(hex)) throw new Exception("Color not valid");

			if (hex.StartsWith("#"))
				hex = hex.Substring(1).ToUpper();

			if (hex.Length != 6) throw new Exception("Color not valid");

			var r = Convert.ToByte(hex.Substring(0, 2), 16);
			var g = Convert.ToByte(hex.Substring(2, 2), 16);
			var b = Convert.ToByte(hex.Substring(4, 2), 16);

			return new UniColor(0, r, g, b);
		}

		private List<FlowMenuItem> _templateMenuItems = new List<FlowMenuItem> {
			new FlowMenuItem(){ Group="Source", Icon="fa fa-server",Color="#5D9CEC" },
			new FlowMenuItem(){ Group="Ingest", Icon="fa fa-cloud-upload",Color="#F6BB42" },
			new FlowMenuItem(){ Group="Filter", Icon="fa fa-sign-in",Color="#888600" },
			new FlowMenuItem(){ Group="Processing", Icon="fa fa-exchange", Color="#5CB36D" },
			new FlowMenuItem(){ Group="Tagging", Icon="fa fa-map-marker", Color="#FC6E51" },
			new FlowMenuItem(){ Group="Control", Icon="fa fa-code", Color="#8CC152" },
			new FlowMenuItem(){ Group="Other", Icon="fa fa-braille", Color="#97c5ff" }
		};

		class FlowMenuItem
		{
			public string Name { get; set; }

			public Type Type { get; set; }

			public string Group { get; set; }

			public string Icon { get; set; }

			public string Color { get; set; }
		}

	}
}
