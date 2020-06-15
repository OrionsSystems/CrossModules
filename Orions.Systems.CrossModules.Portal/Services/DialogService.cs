using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Orions.Common;
using Orions.Node.Common;
using Orions.SDK;

namespace Orions.Systems.CrossModules.Portal.Services
{
	public class DialogService : IDialogService
	{
		bool IDialogService.Confirm(string message, EventHandler<DialogClosedEvent> closed, string OKButtonText, string CancelButtonText, int width)
		{
			throw new NotImplementedException();
		}

		void IDialogService.Inform(string message, bool error, int width)
		{
			throw new NotImplementedException();
		}

		Response IDialogService.MultiOptionConfirm(string message, EventHandler<DialogClosedEvent> closed, DialogServiceOption[] options, string OKButtonText, string CancelButtonText, int width)
		{
			throw new NotImplementedException();
		}

		bool IDialogService.OpenFileDialog(string title, string filter, string defaultExt, out string fileName)
		{
			throw new NotImplementedException();
		}

		bool IDialogService.OpenFilesDialog(string title, string filter, string defaultExt, out string[] fileNames, out string[] safeFileNames)
		{
			throw new NotImplementedException();
		}

		bool IDialogService.OpenFileStreams(string title, out string[] fileNames, out string[] safeFileNames, out Stream[] fileStreams)
		{
			throw new NotImplementedException();
		}

		bool IDialogService.OpenFolderDialog(string title, out string folderName)
		{
			throw new NotImplementedException();
		}

		bool IDialogService.OpenFolderDialogExt(string title, out string folderName)
		{
			throw new NotImplementedException();
		}

		bool IDialogService.SaveFile(List<HyperDocument> doc, string filter, string defaultExt, bool addExtension)
		{
			throw new NotImplementedException();
		}

		bool IDialogService.ShowDialogue(DialogueTypes type, IHyperArgsSink store, object source, Action callback)
		{
			throw new NotImplementedException();
		}

		void IDialogService.ShowMessage(string message)
		{
			throw new NotImplementedException();
		}
	}
}
