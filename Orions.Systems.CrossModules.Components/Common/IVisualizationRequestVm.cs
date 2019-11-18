using Orions.Infrastructure.HyperMedia;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public interface IVisualizationRequestVm
	{
		CrossModuleVisualizationRequest VisualizationRequest { get; set; }
	}
}
