using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.Services;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class BlazorDialogService : IDialogService
	{
		private readonly IPopupService _popupService;

		public BlazorDialogService(IPopupService popupService)
		{
			_popupService = popupService;
		}

		public async Task<bool> ConfirmTagsDeletion(IEnumerable<TagModel> tagModel)
		{
			var result = await this._popupService.ShowConfirmation("Tag removal", "Do you really want to remove this tag?");

			return result;
		}

		public async Task<bool> DisplayAlertAsync(string title, string message, string okBtnCaption, string cancelButton)
		{
			return await _popupService.ShowConfirmation(title, message, okBtnCaption, cancelButton);
		}

		public async Task DisplayAlertAsync(string title, string message, string okBtnCaption)
		{
			await _popupService.ShowAlert(title, message, okBtnCaption);
		}

		public Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton, CancellationToken cancellationToken)
		{
			Debug.WriteLine($"BlazorDialogService message: {title} : {message}");
			return Task.FromResult(true);
		}

		public Task DisplayAlertAsync(string title, string message, string cancelButton, CancellationToken cancellationToken)
		{
			Debug.WriteLine($"BlazorDialogService message: {title} : {message}");
			return Task.FromResult(true);
		}

		public async Task<bool> DisplaySessionContinuationOptions(TimeSpan timeout)
		{
			var result = await this._popupService.ShowSessionIsOver((int)(timeout.TotalSeconds));

			return result;
		}
	}
}
