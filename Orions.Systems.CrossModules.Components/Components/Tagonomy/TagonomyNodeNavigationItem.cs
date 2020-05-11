using Orions.Common;
using Orions.Infrastructure.HyperSemantic;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Orions.Systems.CrossModules.Components
{
	public class TagonomyNodeNavigationItem
	{
		public string Id { get; set; }
		public string ParentId { get; set; }
		public string Name { get; set; }
		public bool Expanded { get; set; } = true;
		public bool HasSubFolders { get; set; }
		public bool Selected { get; set; }

		public TagonomyNode Node { get; set; }
	}
}
