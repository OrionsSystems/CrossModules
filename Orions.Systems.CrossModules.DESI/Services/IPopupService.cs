using System.Threading.Tasks;
using Orions.Systems.Desi.Common.TagonomyExecution;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public interface IPopupService
	{
		void RegisterTagonomyNodePopper(TagonomyNodeModel node, string referenceElId);
		Task ShowAlert(string title, string question);
		Task ShowAlert(string title, string question, string okBtnCaption);
		Task<bool> ShowConfirmation(string title, string question);
		Task<bool> ShowConfirmation(string title, string question, string okBtnCaption, string cancelBtnCaption);
		Task<bool> ShowSessionIsOver(int secondsToTimeout);
	}
}