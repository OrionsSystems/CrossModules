using System;

namespace Orions.Systems.CrossModules.Timeline.Models
{
	public abstract class SuperFilterModel
	{

		public string ServerUri { get; set; }

		public string TagId { get; set; }

		public int TimeOutInSeconds { get; set; }

		protected SuperFilterModel()
		{
			TimeOutInSeconds = 10;
		}
	}
}
