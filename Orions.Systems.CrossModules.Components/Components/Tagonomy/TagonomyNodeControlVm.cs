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

		public TagonomyNode Node { get; set; }

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

		private async Task PopulateData()
		{ 
		
		}

	}
}
