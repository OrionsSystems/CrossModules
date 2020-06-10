using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orions.Common;
using Orions.Node.Common;
using Orions.SDK;

namespace Orions.Systems.CrossModules.Portal.Services
{
	public class DialogService : IDialogService
	{
		public bool Confirm(string message, EventHandler<DialogClosedEvent> closed, string OKButtonText = "OK", string CancelButtonText = "Cancel", int width = 400)
		{
			throw new NotImplementedException();
		}

		public void Inform(string message, bool error = false, int width = 400)
		{
			throw new NotImplementedException();
		}

		public Response MultiOptionConfirm(string message, EventHandler<DialogClosedEvent> closed, DialogServiceOption[] options, string OKButtonText = "OK", string CancelButtonText = "Cancel", int width = 400)
		{
			throw new NotImplementedException();
		}

		public bool OpenFileDialog(string title, string filter, string defaultExt, out string fileName)
		{
			throw new NotImplementedException();
		}

		public bool SaveFile(List<HyperDocument> doc, string filter, string defaultExt, bool addExtension = true)
		{
			throw new NotImplementedException();
		}

		public void ShowMessage(string message)
		{
			throw new NotImplementedException();
		}
	}
}
