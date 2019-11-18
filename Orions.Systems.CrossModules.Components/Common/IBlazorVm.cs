using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public interface IBlazorVm
	{
		BaseBlazorComponent OwnerComponent { get; set; }
	}
}
