using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.CrossModules.Desi.Services;
using Orions.Systems.Desi.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Components.ConfirmationPopup
{
	public class ConfirmationPopupBase : ComponentBase
	{
		[Inject]
		public BlazorDependencyResolver DependencyResolver { get; set; }

		public string Title { get; set; }
		public string Question { get; set; }

		public bool IsDisplayed { get; set; }

		protected override Task OnInitializedAsync()
		{
			var popupService = new PopupService(this);

			DependencyResolver.SetPopupService(popupService);

			return base.OnInitializedAsync();
		}

		private TaskCompletionSource<bool> _tcs;
		public async Task<bool> ShowYesNoModal()
		{
			this.IsDisplayed = true;
			_tcs = new TaskCompletionSource<bool>();

			StateHasChanged();
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
