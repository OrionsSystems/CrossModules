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
	public class CounterSampleVm : BlazorVm
	{
		public ViewModelProperty<string> TimeValueProp { get; set; } = new ViewModelProperty<string>("...");

		public BlazorCommand CommandStop { get; set; } = new BlazorCommand();

		bool _isStopped = false;

		public CounterSampleVm()
		{
			CommandStop.Delegate = OnCommand1;

			Task.Factory.StartNew(() =>
			{
				while (_isStopped == false)
				{
					TimeValueProp.Value = DateTime.Now.ToString();
					Thread.Sleep(1000);
				}
			});
		}

		public void OnTextEdited(/* */)
		{
		}

		private void OnCommand1(DefaultCommand command, object parameter)
		{
			var z = this.OwnerComponent?.GetObjectFromQueryString<CrossModuleVisualizationRequest>("request");

			_isStopped = true;
		}

		//public Task HandleEventAsync(EventCallbackWorkItem item, object arg)
		//{
		//	return Task.CompletedTask;
		//}
	}
}
