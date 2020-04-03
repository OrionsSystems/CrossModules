using Orions.Systems.CrossModules.Desi.Components.ConfirmationPopup;
using Orions.Systems.CrossModules.Desi.Components.SessionIsOverPopup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class PopupService
	{
		private ConfirmationPopupBase _popupComponent;

		public SessionIsOverPopup SessionIsOverPopup { get; set; }

		public PopupService(ConfirmationPopupBase popupComponent)
		{
			this._popupComponent = popupComponent;
		}

		public async Task<bool> ShowConfirmation(string title, string question)
		{
			_popupComponent.Title = title;
			_popupComponent.Message = question;
			var result = await _popupComponent.ShowYesNoModal();

			return result;
		}

		public async Task ShowAlert(string title, string question)
		{
			_popupComponent.Title = title;
			_popupComponent.Message = question;
			await _popupComponent.ShowOkModal();
		}

		public async Task ShowAlert(string title, string question, string okBtnCaption)
		{
			_popupComponent.OkCaption = okBtnCaption;
			await this.ShowAlert(title, question);
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
