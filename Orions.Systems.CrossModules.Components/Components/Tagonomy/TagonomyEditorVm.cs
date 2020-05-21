using Microsoft.AspNetCore.Components;

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
		public enum Modes
		{
			Full,
			Limited_User,
		}

		public string TagonomyId { get; set; }

		public Tagonomy Source { get; set; }

		public Modes Mode { get; set; }

		public List<TagonomyNodeNavigationItem> TagonomyNav { get; set; } = new List<TagonomyNodeNavigationItem>();

		public TagonomyNodeViewItem RootView { get; set; }

		public bool IsLoadedData { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public TagonomyNode SelectedTagonomyNode { get; set; }

		public EventCallback<Tagonomy> OnShowVizList { get; set; }

		public bool IsShowCreateNodeModal { get; set; }

		public string CreatedTagonomyNodeName { get; set; }

		public TagonomyNodeViewItem SelectedItem { get; set; }

		public TagonomyEditorVm()
		{

		}

		public async Task Init()
		{
			if (HyperStore == null) return;

			await PopulateData();

			SelectedTagonomyNode = RootView?.Node;
			SelectedItem = PreCreateView(null, SelectedTagonomyNode);

			IsLoadedData = true;
		}

		public void OnSelect(NodeSelectEventArgs args)
		{
			var selectedId = args.NodeData.Id;
			SelectedTagonomyNode = TagonomyNav.FirstOrDefault(it => it.Id == selectedId)?.Node;
			SelectedItem = PreCreateView(null, SelectedTagonomyNode);
		}

		public void CreateTagonomy()
		{
			//Show Create Node modal window
			IsShowCreateNodeModal = true;
			CreatedTagonomyNodeName = String.Empty;
		}

		public async Task CreateTagonomyNodeAsync()
		{
			if (string.IsNullOrEmpty(CreatedTagonomyNodeName))
			{
				await OnToastMessage.InvokeAsync(new ToastMessage("Tagonomy node name couldn't be empty.", ToastMessageType.Error));
				return;
			}

			CreateNode(false, null);

			IsShowCreateNodeModal = false;
		}

		void CreateNode(bool isShortcut, string name)
		{
			var node = new TagonomyNode() { Name = "{New Node}" };

			if (string.IsNullOrEmpty(name) == false)
				node.Name = CreatedTagonomyNodeName;

			if (this.Mode == Modes.Limited_User)
				node.Type = TagonomyNode.NodeTypes.User;

			if (Source != null)
				node.TagonomyId = Source.Id;


			if (SelectedItem != null)
			{
				var pathId = SelectedItem.Node.ElementsArray.Where(it => it is PathNodeElement).Select(it => it.Id).LastOrDefault(); // Get the Id of the last Path Element.

				var leg = SelectedItem.Node.AddChildNode(pathId, node.Id, isShortcut ? PathNodeElement.Leg.Types.Shortcut : PathNodeElement.Leg.Types.Default);
			}

			if (Source != null)
				Source.AddNode(node);

			RefreshNavigation();
		}

		public async Task DeleteTagonomy()
		{
			if (SelectedItem == null)
			{
				await OnToastMessage.InvokeAsync(new ToastMessage("You have to select tagonomy node first!", ToastMessageType.Warning));
				return;
			}

			if (SelectedItem.Node == null)
			{
				await OnToastMessage.InvokeAsync(new ToastMessage("Selected tagonomy node is empty!", ToastMessageType.Error));
				return;
			}

			var node = SelectedItem.Node as TagonomyNode;

			if (this.Mode == Modes.Limited_User && node.Type != TagonomyNode.NodeTypes.User)
			{
				await OnToastMessage.InvokeAsync(new ToastMessage("User Mode - can only remove and manage user generated nodes.", ToastMessageType.Warning));
				return;
			}

			if (Source != null)
			{
				Source.RemoveNode(node);
			}

			RefreshNavigation();
		}

		public async Task TagonomyUp()
		{
			if (SelectedItem == null)
			{
				await OnToastMessage.InvokeAsync(new ToastMessage("You have to select tagonomy node first!", ToastMessageType.Warning));
				return;
			}

			var parentView = SelectedItem?.ParentView;
			var parentNode = parentView != null ? parentView.Node : null;
			var node = SelectedItem.Node;

			if (parentNode != null && node != null)
			{
				var pathElement = parentNode.GetElements<PathNodeElement>().FirstOrDefault(it => it.Legs.Any(it2 => it2.TargetNodeId == node.Id));
				if (pathElement != null)
				{
					var leg = pathElement.Legs.First(it => it.TargetNodeId == node.Id);
					pathElement.MoveLeg(leg, -1);
				}

				// We need to manually update the view model also, since it wont re-generate.
				int index = SelectedItem.ParentView.Children.IndexOf(SelectedItem);
				if (index > 0)
				{
					SelectedItem.ParentView.Children.Remove(SelectedItem);
					SelectedItem.ParentView.Children.Insert(Math.Min(SelectedItem.ParentView.Children.Count, index - 1), SelectedItem);

					RefreshNavigation();
				}
			}
		}

		public async Task TagonomyDown()
		{
			if (SelectedItem == null)
			{
				await OnToastMessage.InvokeAsync(new ToastMessage("You have to select tagonomy node first!", ToastMessageType.Warning));
				return;
			}

			var node = SelectedItem.Node;
			var parentView = SelectedItem.ParentView;
			var parentNode = parentView != null ? parentView.Node : null;

			if (parentNode != null && node != null)
			{
				var pathElement = parentNode.GetElements<PathNodeElement>().FirstOrDefault(it => it.Legs.Any(it2 => it2.TargetNodeId == node.Id));
				if (pathElement != null)
				{
					var leg = pathElement.Legs.First(it => it.TargetNodeId == node.Id);
					pathElement.MoveLeg(leg, 1);
				}

				// We need to manually update the view model also, since it wont re-generate.
				int index = SelectedItem.ParentView.Children.IndexOf(SelectedItem);
				if (index < SelectedItem.ParentView.Children.Count - 1)
				{
					SelectedItem.ParentView.Children.Remove(SelectedItem);
					SelectedItem.ParentView.Children.Insert(Math.Min(SelectedItem.ParentView.Children.Count, index + 1), SelectedItem);

					RefreshNavigation();
				}
			}
		}

		public async Task Refresh()
		{
			await PopulateData();
		}

		public async Task ShowVizListAsync()
		{
			await OnShowVizList.InvokeAsync(Source);
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

			RefreshNavigation();
		}

		private void RefreshNavigation()
		{
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

			if (item.Children != null)
			{
				foreach (var childItem in item.Children)
				{
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
