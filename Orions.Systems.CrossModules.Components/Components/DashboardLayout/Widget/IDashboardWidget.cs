using Orions.Common;
using Orions.Node.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	public interface IDashboardWidget : IConfig
	{
		string Label { get; set; }
	}
}
