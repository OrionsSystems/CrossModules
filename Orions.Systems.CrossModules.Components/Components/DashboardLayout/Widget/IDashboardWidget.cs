using Orions.Node.Common;
using System;

namespace Orions.Systems.CrossModules.Components
{
	public interface IDashboardWidget
	{
		string Id { get; set; }

		string Label { get; set; }

		/// <summary>
		/// Abstract method to get the view component type to use
		/// </summary>
		/// <returns></returns>
		Type GetViewComponent();
	}
}
