using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using System.Threading.Tasks;
using System.Timers;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;

namespace Orions.Systems.CrossModules.Desi.Components.SessionIsOverPopup
{
	public class SessionIsOverPopupBase : BaseComponent
	{
		public bool IsDisplayed { get; set; }
		public int SecondsToTimeOutLeft { get; set; }

		private TaskCompletionSource<bool> _tcs;
		private Timer _timer;

		public async Task<bool> Show(int secondsToTimeOut)
		{
			this.IsDisplayed = true;
			_tcs = new TaskCompletionSource<bool>();
			UpdateState();

			using (_timer = new Timer(1000))
			{
				SecondsToTimeOutLeft = secondsToTimeOut;
				_timer.AutoReset = true;
				_timer.Elapsed += (s, e) =>
				{
					SecondsToTimeOutLeft -= 1;
					UpdateState();

					if (SecondsToTimeOutLeft == 0)
					{
						OnSaveAndExit();
					}
				};
				_timer.Start();

				var result = await _tcs.Task;

				return result;
			}
		}

		public void OnContinue()
		{
			this.IsDisplayed = false;
			_tcs.SetResult(true);

			UpdateState();
		}

		public void OnSaveAndExit()
		{
			this.IsDisplayed = false;
			_tcs.SetResult(false);
			UpdateState();
		}
	}
}
