using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public static class WindowResizeDispatcher
	{
		public static event Func<Task> WindowResize;
		public static int? WindowWidth { get; private set; }
		public static int? WindowHeight { get; private set; }

		[JSInvokable]
		public static async Task RaiseWindowResizeEvent(int width, int height)
		{
			WindowWidth = width;
			WindowHeight = height;
			bool canRaiseEvent = true;
			try
			{
				_ = WindowResize.Target.ToString();
			}
			catch (Exception ex)
			{
				canRaiseEvent = false;
			}

			if (canRaiseEvent)
			{
				await WindowResize?.Invoke();
			}
		}
	}
}
