using Orions.Systems.CrossModules.Desi.Components.ConfirmationPopup;
using Orions.Systems.CrossModules.Desi.Components.Popper;
using Orions.Systems.CrossModules.Desi.Components.SessionIsOverPopup;
using Orions.Systems.Desi.Common.TagonomyExecution;
using System;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.Components.Desi.Services;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class PopupService : IPopupService
	{
		private ConfirmationPopupBase ConfirmationPopupComponent { get; set; }
		private PopperServiceComponentBase PopperServiceComponent { get; set; }
		private SessionIsOverPopup SessionIsOverPopup { get; set; }

		private bool _initialized;

		public void Init(ConfirmationPopupBase confirmationPopupBase,
			PopperServiceComponentBase popperServiceComponentBase,
			SessionIsOverPopup sessionIsOverPopup)
		{
			ConfirmationPopupComponent = confirmationPopupBase;
			PopperServiceComponent = popperServiceComponentBase;
			SessionIsOverPopup = sessionIsOverPopup;
			_initialized = true;
		}

		public async Task<bool> ShowConfirmation(string title, string question)
		{
			CheckInit();
			ConfirmationPopupComponent.OkCaption = "Yes";
			ConfirmationPopupComponent.CancelCaption = "No";
			ConfirmationPopupComponent.Title = title;
			ConfirmationPopupComponent.Message = question;
			var result = await ConfirmationPopupComponent.ShowYesNoModal();

			return result;
		}

		public async Task ShowAlert(string title, string question)
		{
			CheckInit();
			ConfirmationPopupComponent.Title = title;
			ConfirmationPopupComponent.Message = question;
			await ConfirmationPopupComponent.ShowOkModal();
		}

		public async Task ShowAlert(string title, string question, string okBtnCaption)
		{
			CheckInit();
			ConfirmationPopupComponent.OkCaption = okBtnCaption;
			await this.ShowAlert(title, question);
		}

		public async Task<bool> ShowConfirmation(string title, string question, string okBtnCaption, string cancelBtnCaption)
		{
			CheckInit();
			ConfirmationPopupComponent.OkCaption = okBtnCaption;
			ConfirmationPopupComponent.CancelCaption = cancelBtnCaption;
			return await this.ShowConfirmation(title, question);
		}

		public async Task<bool> ShowSessionIsOver(int secondsToTimeout)
		{
			return await SessionIsOverPopup.Show(secondsToTimeout);
		}

		public void RegisterTagonomyNodePopper(TagonomyNodeModel node, string referenceElId) => PopperServiceComponent.RegisterTagonomyNodePopper(node, referenceElId);

		private void CheckInit()
		{
			if (!_initialized)
			{
				throw new InvalidOperationException($"You should call {nameof(Init)} method first in order to use {nameof(PopupService)}.");
			}
		}
	}
}
