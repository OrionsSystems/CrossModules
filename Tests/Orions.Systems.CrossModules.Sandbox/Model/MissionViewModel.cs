using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Orions.SDK.Utilities;

namespace Orions.Systems.CrossModules.Sandbox.Model
{
	public class MissionViewModel
	{
		public HyperDocument Document { get; set; }

		public HyperMission Mission { get; set; }

		public string MissionId { get; set; }

		public string Name { get; set; }

		public bool IsActive { get; set; }

		public string MissionInstanceId { get; set; }

		public HyperMissionInstanceStatus InstanceStatus { get; set; }

		public string InstanceDescription { get; set; }

		public string Status { get; set; }
	}
}
