using Microsoft.AspNetCore.Components;
using Orions.Infrastructure.HyperSemantic;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.TagonomyExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;

namespace Orions.Systems.CrossModules.Desi.Components.Popper
{
	public class PopperServiceComponentBase : BaseComponent
	{
		public List<(TagonomyNodeModel Node, string ReferenceElId)> TagonomyNodes { get; private set; } = new List<(TagonomyNodeModel, string)>();

		public void RegisterTagonomyNodePopper(TagonomyNodeModel node, string referenceElId)
		{
			if(!TagonomyNodes.Any(n => n.Node.Id == node.Id))
			{
				TagonomyNodes.Add((node, referenceElId));
			}

			UpdateState();
		}
	}
}
