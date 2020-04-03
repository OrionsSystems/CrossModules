using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Components.Popup.Alert;
using Orions.Systems.Desi.Common.Services;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class BlazorDialogService : IDialogService
	{
		private readonly PopupService _popupService;

		public BlazorDialogService(PopupService popupService)
		{
			_popupService = popupService;
		}

		public Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton)
		{
			Debug.WriteLine($"BlazorDialogService message: {title} : {message}");
			return Task.FromResult(true);
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
	}
}
