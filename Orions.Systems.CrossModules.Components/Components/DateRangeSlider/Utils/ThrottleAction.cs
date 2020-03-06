using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components.Components.DateRangeSlider.Utils
{
	public class ThrottleAction<T>
	{
		private Action<T> _callback;
		private int _timeout;
		private object _raiseLock = new object();
		private T _callbackParam;
		private System.Timers.Timer _timer;

		public ThrottleAction(Action<T> eventCallback, int timeOut)
		{
			this._callback = eventCallback;
			this._timeout = timeOut > 0 ? timeOut : 200;
		}


		public void Invoke(T callbackParam)
		{
			lock (_raiseLock)
			{
				_callbackParam = callbackParam;

				if (_timer == null)
				{
					_timer = new System.Timers.Timer(_timeout);
					_timer.Elapsed += TimerElapsed;
					_timer.AutoReset = false;
					_timer.Start();
				}
				else
				{
					_timer.Stop();
					_timer.Start();
				}
			}
		}

		public void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			_timer.Dispose();
			_timer = null;

			this._callback.Invoke(_callbackParam);
		}
	}
}
