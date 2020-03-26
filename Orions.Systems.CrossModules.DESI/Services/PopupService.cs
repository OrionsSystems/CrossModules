using Orions.Systems.CrossModules.Desi.Components.ModalPopup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class PopupService
	{
		private ModalPopupBase _popupComponent;

		public PopupService(ModalPopupBase popupComponent)
		{
			this._popupComponent = popupComponent;
		}

		public async Task<bool> ShowConfirmation(string title, string question)
		{
			_popupComponent.Title = title;
			_popupComponent.Question = question;
			var result = await _popupComponent.ShowYesNoModal();

			return result;
		}
	}
}
