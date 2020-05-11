using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;
using Syncfusion.EJ2.Blazor.Navigations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class TagonomyEditorVm : BlazorVm
	{
		public string TagonomyId { get; set; }

		public Tagonomy Source { get; set; }


		public List<TagonomyNodeNavigationItem> TagonomyNav { get; set; } = new List<TagonomyNodeNavigationItem>();

		public TagonomyNodeViewItem RootView { get; set; }

		public bool IsLoadedData { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public TagonomyNode SelectedTagonomyNode { get; set; }


		public TagonomyEditorVm()
		{

		}

		public async Task Init()
		{
			if (HyperStore == null) return;

			await PopulateData();

			SelectedTagonomyNode = RootView?.Node;

			IsLoadedData = true;
		}

		public void OnSelect(NodeSelectEventArgs args)
		{
			var selectedId = args.NodeData.Id;

			SelectedTagonomyNode = TagonomyNav.FirstOrDefault(it => it.Id == selectedId)?.Node;

		}

		public async Task CreateTagonomy()
		{

		}

		public async Task DeleteTagonomy()
		{

		}

		public async Task TagonomyUp()
		{

		}

		public async Task TagonomyDown()
		{

		}

		public async Task Refresh()
		{

		}

		private async Task PopulateData()
		{
			if (HyperStore == null || string.IsNullOrWhiteSpace(TagonomyId)) return;

			var documentId = HyperDocumentId.Create<Tagonomy>(TagonomyId);
			var argTagonomy = new RetrieveHyperDocumentArgs(documentId);
			var doc = await HyperStore.ExecuteAsync(argTagonomy);

			if (argTagonomy.ExecutionResult.IsNotSuccess)
				return;

			Source = doc?.GetPayload<Tagonomy>();

			RootView = PreCreateView(null, Source.RootNode);

			PopulateNavigation(RootView);
		}


		public TagonomyNodeViewItem PreCreateView(PathNodeElement.Leg leg, TagonomyNode node)
		{
			var view = new TagonomyNodeViewItem(node);
			view.Leg = leg;

			var tagonomy = this.Source;

			if (leg == null || leg.Type != PathNodeElement.Leg.Types.Shortcut) // Shortcuts show no children.
			{
				foreach (var childNodeLeg in node.GetDirectChildrenLegs())
				{
					var childNode = tagonomy.GetNode(childNodeLeg.TargetNodeId);
					if (childNode == null)
						continue;

					TagonomyNodeViewItem childView = PreCreateView(childNodeLeg, childNode);
					view.AddChild(childView);
				}
			}

			return view;
		}

		private void PopulateNavigation(TagonomyNodeViewItem item)
		{
			var view = GetTagonomyNav(item);
			TagonomyNav.Add(view);

			if (item.Children != null) {
				foreach (var childItem in item.Children) {
					if (childItem.Children != null) 
						PopulateNavigation(childItem);
				}
			}

		}

		private TagonomyNodeNavigationItem GetTagonomyNav(TagonomyNodeViewItem item)
		{

			if (item == null) throw new Exception("Missing tagonomy node data");

			return new TagonomyNodeNavigationItem
			{
				Id = item.Id,
				Name = item.Name,
				Expanded = true,
				ParentId = item.ParentView?.Id,
				HasSubFolders = item.Children.Count > 0,
				Node = item.Node
			};
		}

	}
}
