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
	public class TagonomyNodeControlVm : BlazorVm
	{
		public IHyperArgsSink HyperStore { get; set; }

		public PropertyGridVm PropertyGridVm { get; set; } = new PropertyGridVm();

		public TagonomyNodeElement SelectedNodeElement { get; set; }

		public bool IsShowProperty { get; set; }

		private TagonomyNode _node;
		public TagonomyNode Node
		{
			get { return _node; }
			set
			{
				_node = value;
				LoadTagonomyElements();
			}
		}

		public List<string> ComboBoxElements { get; set; }

		public string SelectedTagonomyAddElement { get; set; }

		public List<TagonomyNodeElement> Elements { get; set; } = new List<TagonomyNodeElement>();

		public TagonomyNodeControlVm()
		{

		}

		public async Task Init()
		{
			if (HyperStore == null) return;

			await PopulateData();

		}

		public string SemanticAlias
		{
			get
			{
				var alias = Node?.GetElement<SemanticAliasNodeElement>();
				if (alias?.Alias == null)
					return "";

				return alias.Alias.Name ?? "";
			}

			set
			{
				var node = Node;
				if (node == null)
				{
					System.Diagnostics.Debug.Assert(false, "Failed to find node instance");
					return;
				}

				var alias = node.ObtainElement<SemanticAliasNodeElement>();
				if (alias.Alias == null)
					alias.Alias = new SemanticAlias();

				alias.Alias.Name = value;
				RaisePropertyChanged(nameof(SemanticAlias));
			}
		}

		public string SemanticNamespace
		{
			get
			{
				var alias = Node?.GetElement<SemanticAliasNodeElement>();
				if (alias?.Alias == null)
					return "";

				return alias.Alias.Namespace ?? "";
			}

			set
			{
				var node = Node;
				if (node == null)
				{
					System.Diagnostics.Debug.Assert(false, "Failed to find node instance");
					return;
				}

				var alias = node.ObtainElement<SemanticAliasNodeElement>();
				if (alias.Alias == null)
					alias.Alias = new SemanticAlias();

				alias.Alias.Namespace = value;
				RaisePropertyChanged(nameof(SemanticAlias));
			}
		}

		public string SemanticGroups
		{
			get
			{
				var alias = Node?.GetElement<SemanticAliasNodeElement>();
				if (alias?.Alias == null)
					return "";

				return string.Join(", ", alias.Alias.Groups) ?? "";
			}

			set
			{
				var node = Node;
				if (node == null)
				{
					System.Diagnostics.Debug.Assert(false, "Failed to find node instance");
					return;
				}

				var alias = node.ObtainElement<SemanticAliasNodeElement>();
				if (alias.Alias == null)
					alias.Alias = new SemanticAlias();

				alias.Alias.Groups = (value ?? string.Empty).Split(',');
				RaisePropertyChanged(nameof(SemanticGroups));

			}
		}

		public void AddTagonomyElement()
		{
			if (string.IsNullOrWhiteSpace(SelectedTagonomyAddElement)) return;

			if (Node == null) return;

			Type type = TagonomyNodeElement.ElementTypes.Where(it => it.Name == SelectedTagonomyAddElement).First();

			TagonomyNodeElement element = (TagonomyNodeElement)Activator.CreateInstance(type);
			Node.AddElement(element);
		}

		public void DeleteTagonomyNodeElement(TagonomyNodeElement element)
		{
			if (Node != null && element != null)
				Node.RemoveElement(element);
		}

		public void UpTagonomyNodeElement(TagonomyNodeElement element)
		{
			if (Node != null && element != null)
				Node.MoveElement(element, true);
		}

		public void DownTagonomyNodeElement(TagonomyNodeElement element)
		{
			if (Node != null && element != null)
				Node.MoveElement(element, false);
		}

		public void ShowPropertyGrid(TagonomyNodeElement item)
		{
			SelectedNodeElement = item;
			IsShowProperty = true;
		}

		public Task<object> LoadPropertyGrid()
		{
			return Task.FromResult<object>(SelectedNodeElement);
		}

		public void OnCancelProperty()
		{
			PropertyGridVm.CleanSourceCache();
			IsShowProperty = false;
		}

		private async Task PopulateData()
		{
			ComboBoxElements = TagonomyNodeElement.ElementTypes.Select(it => it.Name).Select(it => it.Replace("Orions.Infrastructure.HyperSemantic", string.Empty)).OrderBy(it => it).ToList();

			LoadTagonomyElements();

		}

		private void LoadTagonomyElements()
		{
			if (Node == null) return;

			Elements.Clear();

			foreach (var element in Node.ElementsArray.Where(it => it != null))
			{
				Elements.Add(element);
			}
		}

	}
}
