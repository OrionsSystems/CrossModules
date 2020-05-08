using Newtonsoft.Json;

using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Components.Model;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Orions.Systems.CrossModules.Components
{
	public class TagonomyDesignerVm : FlowDesignerVm
	{
		public string TagonomyId { get; set; }

		public Tagonomy Selected { get; set; }

		private List<TagonomiesLink> Links { get; set; } = new List<TagonomiesLink>();
		private Tagonomy _rootTagonomy;
		private Tagonomy[] _allTagonomies;
		private Dictionary<string, Tagonomy> _unprocessedSourceTagonomies = new Dictionary<string, Tagonomy>();
		private Dictionary<string, Tagonomy> _processedSourceTagonomies = new Dictionary<string, Tagonomy>();

		public TagonomyDesignerVm()
		{

		}

		public virtual async Task Init()
		{
			if (HyperStore == null) return;

			await PopulateData();

			PopulateDesignerData();
		}

		public override void ShowPropertyGrid(string tagonomyId)
		{
			PropertyGridVm.CleanSourceCache();

			Selected = _allTagonomies.FirstOrDefault(it => it.Id == tagonomyId);

			IsShowProperty = true;
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

		double _x = 0;
		double _y = 0;
		int _Xoffset = 250;
		int _Yoffset = 150;
		private void PopulateDesignerData()
		{
			DesignData = new FlowDesignData();

			if (_rootTagonomy == null) throw new Exception("Missing source!");

			DesignData.FlowName = _rootTagonomy.Name;
			DesignData.FlowId = _rootTagonomy.Id;

			_x = _Xoffset;
			_y = _Yoffset;

			//  menu componnets
			foreach (var tagItem in _allTagonomies) {

				var designNodeConfiguration = new FlowDesignNodeConfiguration()
				{
					Id = tagItem.Name,
					TypeFull = tagItem.Id,
					Title = tagItem.Name,
					Input = 1,
					Output = 1,
					Group = "Tagonomies",
					Color = "#5D9CEC",
					Icon = "fa fa-map-marker",
					//Html = nodeGroupType.Html
				};

				DesignData.NodeConfigurations.Add(designNodeConfiguration);
			}

			if (!Links.Any())
			{
				var component = new FlowDesignComponent()
				{
					X = Convert.ToInt64(_x),
					Y = Convert.ToInt64(_y),// TODO fix me!
					Name = _rootTagonomy.Name,
					Id = _rootTagonomy.Id,
					Type = _rootTagonomy.Name, //_rootTagonomy.GetType().ToString(),
					State = new FlowState()
					{
						Text = _rootTagonomy.Name,
						Color = "#FC6E51"
					},
					Group = "Tagonomies"
				};

				DesignData.Components.Add(component);

				return;
			}

			//add node

			var sources = Links.GroupBy(x => x.SourceTagonomy).ToArray();

			sources = sources.OrderBy(x => Links.Count(l => l.TargetTagonomy.Id == x.Key.Id)).ToArray();


			foreach (var sourceLinks in sources)
			{
				var source = sourceLinks.Key;

				var component = DesignData.Components.FirstOrDefault(it => it.Id == source.Id);
				if (component == null)
				{
					component = new FlowDesignComponent()
					{
						X = Convert.ToInt64(_x),
						Y = Convert.ToInt64(_y),// TODO fix me!
						Name = source.Name,
						Id = source.Id,
						Type = source.Name, //_rootTagonomy.GetType().ToString(),
						State = new FlowState()
						{
							Text = sourceLinks.Key.Name,
							Color = "#FC6E51"
						},
						Group = "Tagonomies"
					};

					DesignData.Components.Add(component);

					_x += _Xoffset;
				}

				var targetsCount = sourceLinks.Count();

				//connections
				foreach (var link in sourceLinks)
				{
					var targetLink = link.TargetTagonomy;
					var sourceLink = link.SourceTagonomy;
					var isForwardLiniking = link.IsForwardLink;

					var targetComponent = DesignData.Components.FirstOrDefault(it => it.Id == targetLink.Id);
					if (targetComponent == null)
					{
						targetComponent = new FlowDesignComponent()
						{
							X = Convert.ToInt64(_x),
							Y = Convert.ToInt64(_y),// TODO fix me!
							Name = targetLink.Name,
							Id = targetLink.Id,
							Type = targetLink.Id, //_rootTagonomy.GetType().ToString(),
							State = new FlowState()
							{
								Text = targetLink.Name,
								Color = "#FC6E51"
							},
							Group = "Tagonomies"
						};

						DesignData.Components.Add(targetComponent);

						_y += _Yoffset;
					}

					if (!isForwardLiniking)
					{
						targetComponent.Connections.Add(new FlowConnection()
						{
							Id = sourceLink.Id,
							Index = "0"
						});
					}
					else {
						component.Connections.Add(new FlowConnection()
						{
							Id = targetLink.Id,
							Index = "0"
						});
					}
					
				}
			}
		}


		public void SaveDesignData(string json)
		{
			throw new NotImplementedException();
		}

		private async Task PopulateData()
		{
			if (HyperStore == null || string.IsNullOrWhiteSpace(TagonomyId)) return;

			var documentId = HyperDocumentId.Create<Tagonomy>(TagonomyId);
			var argTagonomy = new RetrieveHyperDocumentArgs(documentId);
			var doc = await HyperStore.ExecuteAsync(argTagonomy);

			if (argTagonomy.ExecutionResult.IsNotSuccess)
				return;

			_rootTagonomy = doc?.GetPayload<Tagonomy>();

			_unprocessedSourceTagonomies.Clear();
			_processedSourceTagonomies.Clear();


			var args = new FindHyperDocumentsArgs(typeof(Tagonomy), true);

			_allTagonomies = ((await this.HyperStore.ExecuteAsyncThrows(args)) ?? new Node.Common.HyperDocument[0])
				.Select(x => x.GetPayload<Tagonomy>())
				.ToArray();

			_unprocessedSourceTagonomies.Add(_rootTagonomy.Id, _rootTagonomy);

			while (_unprocessedSourceTagonomies.Any())
			{
				foreach (var unprocTagonomie in _unprocessedSourceTagonomies.Select(x => x.Value).ToArray())
				{
					var linkingNodesInner = GetLinkingNodes(unprocTagonomie);
					ProcessLinkingNodes(linkingNodesInner, unprocTagonomie, true);
				}
			}

			GoBackwards();
		}

		private void GoBackwards()
		{
			// Was not processed as source and has outgoing references
			var filteredTagonomies = _allTagonomies
				.Where(x => !_processedSourceTagonomies.ContainsKey(x.Id))
				.Where(x => x.Nodes.Any(n => n.ElementsArray.Any(e => e is ReferenceNodeElement)))
				.ToList();

			bool anyResults = true;
			do
			{
				var targetIds = _processedSourceTagonomies.Select(x => x.Key).ToArray();

				var tagonomiesWithCurrentTargets = filteredTagonomies
					.Where(x => x.Nodes.Any(n => n.ElementsArray.Any(el => (el is ReferenceNodeElement refEl) && targetIds.Contains(refEl.TagonomyDocumentId.Id)))).ToArray();

				if (tagonomiesWithCurrentTargets.Any())
				{
					foreach (var tagonomy in tagonomiesWithCurrentTargets)
					{
						filteredTagonomies.Remove(tagonomy);

						var linkingNodes = GetLinkingNodes(tagonomy);
						ProcessLinkingNodes(linkingNodes, tagonomy, false);
					}
				}
				else
				{
					anyResults = false;
				}
			}
			while (anyResults);
		}

		private LinkingNode[] GetLinkingNodes(Tagonomy tagonomy)
		{
			var nodes = tagonomy.Nodes
				.Where(x => x.ElementsArray.Any(e => e is ReferenceNodeElement))
				.Select(x => new LinkingNode
				{
					Node = x,
					References = x.GetElements<ReferenceNodeElement>()
				}).ToArray();

			return nodes;
		}

		/// <summary>
		/// Processes all linking nodes of a single source Tagonomy.
		/// </summary>
		private void ProcessLinkingNodes(LinkingNode[] linkingNodes, Tagonomy sourceTagonomy, bool forwardLinking)
		{
			if (sourceTagonomy == null && linkingNodes.Any())
			{
				var id = linkingNodes.First().Node.TagonomyId;
				sourceTagonomy = _allTagonomies.FirstOrDefault(x => x.Id == id);
			}

			if (sourceTagonomy == null)
				return; // TODO: Log error

			if (forwardLinking)
			{
				_unprocessedSourceTagonomies.Remove(sourceTagonomy.Id);
			}
			_processedSourceTagonomies.Add(sourceTagonomy.Id, sourceTagonomy);

			foreach (var linkingNode in linkingNodes)
			{
				foreach (var reference in linkingNode.References)
				{
					var targetTagonomy = _allTagonomies.FirstOrDefault(x => x.Id == reference.TagonomyDocumentId.Id);

					if (targetTagonomy == null)
						continue; // TODO: Log error

					var link = Links.FirstOrDefault(x => x.SourceTagonomy == sourceTagonomy && x.TargetTagonomy == targetTagonomy);

					if (link == null)
					{
						link = new TagonomiesLink
						{
							SourceTagonomy = sourceTagonomy,
							TargetTagonomy = targetTagonomy,
							IsForwardLink = forwardLinking
						};
						Links.Add(link);

						if (forwardLinking)
						{
							if (!_unprocessedSourceTagonomies.ContainsKey(targetTagonomy.Id) && !_processedSourceTagonomies.ContainsKey(targetTagonomy.Id))
								_unprocessedSourceTagonomies.Add(targetTagonomy.Id, targetTagonomy);
						}
					}

					if (!link.ReferencedNodes.Any(r => r.ReferenceId == reference.Id))
					{
						var referenceNode = new TagonomiesLink.ReferenceSlim
						{
							ReferenceId = reference.Id,
							IsMandatory = reference.IsMandatory,
							SourceNode = linkingNode.Node
						};
						link.ReferencedNodes.Add(referenceNode);
					}
				}
			}
		}


		public class TagonomiesLink
		{
			public class ReferenceSlim
			{
				public string ReferenceId { get; set; }
				public TagonomyNode SourceNode { get; set; }
				public bool IsMandatory { get; set; }
			}

			public Tagonomy SourceTagonomy { get; set; }
			public Tagonomy TargetTagonomy { get; set; }
			public List<ReferenceSlim> ReferencedNodes { get; set; } = new List<ReferenceSlim>();

			public bool IsForwardLink { get; set; }
		}

		private class LinkingNode
		{
			public TagonomyNode Node { get; set; }

			public ReferenceNodeElement[] References { get; set; }
		}
	}
}
