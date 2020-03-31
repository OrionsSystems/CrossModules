using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class TabContainerData
	{
		public string Name { get; set; }

		public bool Active { get; set; }

		public bool Enable { get; set; }

		public string Class { get; set; }

		public string AnimationClass { get; set; } 

		public string OutTransitions { get; set; }

		public string InTransitions { get; set; }

		public bool Action { get; set; }
	}
}
