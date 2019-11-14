using System;

namespace Orions.Systems.CrossModules.Components
{
	public static class IdGeneratorHelper
	{
		public static string Generate(string prefix)
		{
			return prefix + Guid.NewGuid();
		}
	}
}
