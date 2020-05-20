using Orions.Systems.CrossModules.Components.Desi.Services;
using Orions.Systems.Desi.Common.TagonomyExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Domain.Desi
{
	public class PopupServiceStub : IPopupService
	{
		public void RegisterTagonomyNodePopper(TagonomyNodeModel node, string referenceElId)
		{
		}

		public Task ShowAlert(string title, string question)
		{
			return Task.CompletedTask;
		}

		public Task ShowAlert(string title, string question, string okBtnCaption)
		{
			return Task.CompletedTask;
		}

		public Task<bool> ShowConfirmation(string title, string question)
		{
			return Task.FromResult(true);
		}

		public Task<bool> ShowConfirmation(string title, string question, string okBtnCaption, string cancelBtnCaption)
		{
			return Task.FromResult(true);
		}

		public Task<bool> ShowSessionIsOver(int secondsToTimeout)
		{
			return Task.FromResult(true);
		}
	}
}
