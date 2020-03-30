using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using System.Timers;

namespace Orions.Systems.CrossModules.Desi.Components.SessionIsOverPopup
{
	public class SessionIsOverPopupBase : ComponentBase
	{
		public bool IsDisplayed { get; set; }
		public int SecondsToTimeOutLeft { get; set; }

		private TaskCompletionSource<bool> _tcs;
		private Timer _timer;

		public async Task<bool> Show(int secondsToTimeOut)
		{
			this.IsDisplayed = true;
			_tcs = new TaskCompletionSource<bool>();
			OnStateHasChanged();

			using (_timer = new Timer(1000))
			{
				SecondsToTimeOutLeft = secondsToTimeOut;
				_timer.AutoReset = true;
				_timer.Elapsed += (s, e) =>
				{
					SecondsToTimeOutLeft -= 1;
					OnStateHasChanged();

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

			OnStateHasChanged();
		}

		public void OnSaveAndExit()
		{
			this.IsDisplayed = false;
			_tcs.SetResult(false);
			OnStateHasChanged();
		}

		private void OnStateHasChanged()
		{
			this.InvokeAsync(() => this.StateHasChanged());
		}
	}
}
