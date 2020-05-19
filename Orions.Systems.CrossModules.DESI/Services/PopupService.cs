using Orions.Systems.CrossModules.Desi.Components.ConfirmationPopup;
using Orions.Systems.CrossModules.Desi.Components.Popper;
using Orions.Systems.CrossModules.Desi.Components.SessionIsOverPopup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class PopupService
	{
		public ConfirmationPopupBase ConfirmationPopupComponent { get; set; }
		public PopperServiceComponentBase PopperServiceComponent { get; set; }

		public SessionIsOverPopup SessionIsOverPopup { get; set; }

		public PopupService()
		{
		}

		public async Task<bool> ShowConfirmation(string title, string question)
		{
			ConfirmationPopupComponent.OkCaption = "Yes";
			ConfirmationPopupComponent.CancelCaption = "No";
			ConfirmationPopupComponent.Title = title;
			ConfirmationPopupComponent.Message = question;
			var result = await ConfirmationPopupComponent.ShowYesNoModal();

			return result;
		}

		public async Task ShowAlert(string title, string question)
		{
			ConfirmationPopupComponent.Title = title;
			ConfirmationPopupComponent.Message = question;
			await ConfirmationPopupComponent.ShowOkModal();
		}

		public async Task ShowAlert(string title, string question, string okBtnCaption)
		{
			ConfirmationPopupComponent.OkCaption = okBtnCaption;
			await this.ShowAlert(title, question);
		}

		public async Task<bool> ShowConfirmation(string title, string question, string okBtnCaption, string cancelBtnCaption)
		{
			ConfirmationPopupComponent.OkCaption = okBtnCaption;
			ConfirmationPopupComponent.CancelCaption = cancelBtnCaption;
			return await this.ShowConfirmation(title, question);
		}

		public async Task<bool> ShowSessionIsOver(int secondsToTimeout)
		{
			if(SessionIsOverPopup != null)
			{
				var result = await SessionIsOverPopup.Show(secondsToTimeout);

				return result;
			}
			else
			{
				throw new Exception($"{nameof(SessionIsOverPopup)} property must be set on {nameof(PopupService)} before using {nameof(ShowSessionIsOver)} method");
			}
		}
	}
}
