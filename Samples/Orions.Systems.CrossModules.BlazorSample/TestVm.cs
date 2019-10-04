using System;
using System.Threading;
using System.Threading.Tasks;
using Orions.Common;

namespace Orions.Systems.CrossModules.BlazorSample
{

	/// <summary>
	/// The ViewModel is what drives the logic behind the User Interface, in a MVVM based interface.
	/// This is a sample Vm that shows how to use the basic common classes to express properties, commands and more.
	/// </summary>
	public class TestVm : BaseVm
	{
		public ViewModelProperty<string> Value1 { get; set; } = new ViewModelProperty<string>("helloo");

		public SupCommand Command1 { get; set; } = new SupCommand();

		bool _isStopped = false;

		public TestVm()
		{
			Command1.Delegate = OnCommand1;

			Task.Factory.StartNew(() =>
			{
				while (_isStopped == false)
				{
					Value1.Value = DateTime.Now.ToString();
					Thread.Sleep(1000);
				}
			});
		}

		private void OnCommand1(DefaultCommand command, object parameter)
		{
			_isStopped = true;
		}

		//public Task HandleEventAsync(EventCallbackWorkItem item, object arg)
		//{
		//	return Task.CompletedTask;
		//}
	}
}
