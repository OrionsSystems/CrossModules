using Orions.Common;
using Orions.Infrastructure.HyperSemantic;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Orions.Systems.CrossModules.Components
{
	public class TagonomyNodeViewItem : NotifyPropertyChanged
	{
		public string Id { get { return Node.Id; } }
		public string ParentId
		{
			get
			{
				if (ParentView == null) return "";
				return ParentView.Id;
			}
		}
		public string Name
		{
			get
			{
				var node = this.Node;
				if (node == null)
					return "";

				string result = "";
				result = this.Node.ToString();

				var sem = node.GetElement<SemanticAliasNodeElement>();
				if (sem != null)
				{
					result += "  " + sem.Alias?.ToString();
				}

				return result;
			}
		}

		public TagonomyNode Node { get; private set; }

		public ObservableCollection<TagonomyNodeViewItem> Children { get; set; }

		public TagonomyNodeViewItem ParentView { get; set; }

		/// <summary>
		/// Leg leading to this view, if available.
		/// </summary>
		public PathNodeElement.Leg Leg { get; set; }


		public TagonomyNodeViewItem(TagonomyNode node)
		{
			this.Children = new ObservableCollection<TagonomyNodeViewItem>();
			this.Children.CollectionChanged += Children_CollectionChanged;

			this.Node = node;
			node.PropertyChanged += node_PropertyChanged;
		}

		void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			RaiseNotifyPropertyChanged("Children");
		}

		public void AddChild(TagonomyNodeViewItem view)
		{
			this.Children.Add(view);
			view.ParentView = this;
		}

		public void RemoveChild(TagonomyNodeViewItem view)
		{
			this.Children.Remove(view);
			view.ParentView = null;
		}

		void node_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			RaiseNotifyPropertyChanged(nameof(Name));
		}

		public List<TagonomyNodeViewItem> AllParentViews()
		{
			List<TagonomyNodeViewItem> result = new List<TagonomyNodeViewItem>();
			TagonomyNodeViewItem parentView = this.ParentView;
			while (parentView != null)
			{
				result.Add(parentView);
				parentView = parentView.ParentView;
			}

			return result;
		}

		public List<TagonomyNodeViewItem> AllChildViews()
		{
			List<TagonomyNodeViewItem> result = new List<TagonomyNodeViewItem>();
			foreach (var node in Children)
			{
				result.Add(node);
				result.AddRange(node.AllChildViews());
			}

			return result;
		}

		public List<TagonomyNode> AllChildNodes()
		{
			List<TagonomyNode> result = new List<TagonomyNode>();
			foreach (var node in Children)
			{
				result.Add(node.Node);
				result.AddRange(node.AllChildNodes());
			}

			return result;
		}
	}
}
