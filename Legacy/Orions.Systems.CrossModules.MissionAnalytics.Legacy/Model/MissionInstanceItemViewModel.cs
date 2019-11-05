using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.MissionAnalytics.Model
{
	public class MissionInstanceItemViewModel
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public string Label { get; set; }

		public DateTime RunAtUTC { get; set; }

		public DateTime? StopAtUTC { get; set; }
	}
}
