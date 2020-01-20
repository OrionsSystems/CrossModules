using Microsoft.AspNetCore.Components;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class MetadataReviewBase : BaseBlazorComponent<MetadataReviewVm>
	{
		//[Parameter]
		//public HyperDocumentId? MetadataSetId { get; set; }

		[Parameter]
		public UniFilterData Filter { get; set; }

		//[Parameter]
		//public int ColumnsNumber { get; set; }

		[Parameter]
		public new MetadataReviewVm Vm { get { return base.Vm; } set { base.Vm = value; } }

		protected override bool AutoCreateVm => false;
	}
}
