using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.CrossModules.Desi.Services;
using Orions.Systems.Desi.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;

namespace Orions.Systems.CrossModules.Desi.Components.ConfirmationPopup
{
	public class ConfirmationPopupBase : BaseComponent
	{
		[Inject]
		public BlazorDependencyResolver DependencyResolver { get; set; }

		public string OkCaption { get; set; }
		public string CancelCaption { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }

		public bool IsDisplayed { get; set; }

		public bool OnlyOkMode { get; set; }

		private TaskCompletionSource<bool> _tcs;
		public async Task<bool> ShowYesNoModal()
		{
			this.IsDisplayed = true;
			this.OnlyOkMode = false;
			_tcs = new TaskCompletionSource<bool>();

			UpdateState();
			var result = await _tcs.Task;

			return result;
		}

		public async Task<bool> ShowOkModal()
		{
			this.IsDisplayed = true;
			this.OnlyOkMode = true;
			_tcs = new TaskCompletionSource<bool>();

			UpdateState();
			var result = await _tcs.Task;

			return result;
		}

		public void OnYes()
		{
			this.IsDisplayed = false;
			_tcs.SetResult(true);
		}

		public void OnNo()
		{
			this.IsDisplayed = false;
			_tcs.SetResult(false);
		}
	}
}
