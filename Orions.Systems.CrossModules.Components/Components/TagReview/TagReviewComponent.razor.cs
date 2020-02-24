﻿using Microsoft.AspNetCore.Components;
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
	public class TagReviewComponentBase : BaseBlazorComponent<TagReviewVm>
	{
		[Parameter]
		public UniFilterData Filter { get; set; }

		protected override bool AutoCreateVm => false;
	}
}
