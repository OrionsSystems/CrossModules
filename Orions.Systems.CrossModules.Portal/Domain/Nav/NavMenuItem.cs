using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal
{
	public class NavMenuItem
	{
		public string Address { get; set; }

		public string Alias { get; set; }

		public string Label { get; set; }

		public string Description { get; set; }

		public string MatIcon { get; set; }

		public bool EnableLeftMenu { get; set; }
	}
}
