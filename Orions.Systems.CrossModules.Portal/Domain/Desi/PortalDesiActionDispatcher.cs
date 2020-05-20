using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.TagonomyExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Domain.Desi
{
	public class PortalDesiActionDispatcher : IActionDispatcher
	{
		private readonly ITagonomyExecutionController _tagonomyExecutionController;

		public PortalDesiActionDispatcher(ITagonomyExecutionController tagonomyExecutionController)
		{
			this._tagonomyExecutionController = tagonomyExecutionController;
		}

		public void Dispatch(IAction action)
		{
			switch (action)
			{
				case ITagonomyExecutionAction tagonomyExecutionAction:
					_tagonomyExecutionController.Dispatch(tagonomyExecutionAction);
					break;
			}
		}

		public void Dispose()
		{
		}
	}
}
