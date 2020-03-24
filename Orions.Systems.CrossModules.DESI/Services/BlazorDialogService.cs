using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Services;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class BlazorDialogService : IDialogService
	{
		public Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton)
		{
			Debug.WriteLine($"BlazorDialogService message: {title} : {message}");
			return Task.FromResult(true);
		}

		public Task DisplayAlertAsync(string title, string message, string cancelButton)
		{
			Debug.WriteLine($"BlazorDialogService message: {title} : {message}");
			return Task.CompletedTask;
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
