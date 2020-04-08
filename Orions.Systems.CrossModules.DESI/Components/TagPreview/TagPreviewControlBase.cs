using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.General;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Common.Tagging;
using Orions.Systems.Desi.Common.TagsExploitation;
using Orions.Systems.Desi.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Components.TagPreview
{
	public class TagPreviewControlBase : DesiBaseComponent<TaggingViewModel>
	{
		[Parameter]
		public IActionDispatcher ActionDispatcher { get; set; }

		protected Guid? CurrentTagonomyInfoShown;

		public void SelectItem(TagModel tag)
		{
			ActionDispatcher.Dispatch(ToggleTagSelectionAction.Create(tag));
		}
	}
}
