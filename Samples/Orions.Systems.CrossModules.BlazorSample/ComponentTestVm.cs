using System;
using System.Threading;
using System.Threading.Tasks;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Components;

namespace Orions.Systems.CrossModules.BlazorSample
{

	/// <summary>
	/// The ViewModel is what drives the logic behind the User Interface, in a MVVM based interface.
	/// This is a sample Vm that shows how to use the basic common classes to express properties, commands and more.
	/// </summary>
	public class ComponentTestVm : BlazorVm
	{
		public string Power { get; set; }

		public ViewModelProperty<string> BlazorBindProperty { get; set; } = new ViewModelProperty<string>("...");

		public ComponentTestVm()
		{
			BlazorBindProperty.ValueChanged += BlazorBindProperty_ValueChanged;
		}

		private void BlazorBindProperty_ValueChanged(INotifyProperty<string> prop, string newValue)
		{
		}
	}
}
