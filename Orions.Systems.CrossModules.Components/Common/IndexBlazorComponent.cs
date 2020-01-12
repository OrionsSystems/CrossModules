using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class IndexBlazorComponent<VmType> : BaseBlazorComponent<VmType>
		where VmType : BaseVm, new()
	{
		public CrossModuleVisualizationRequest VisualizationRequest { get; set; }

		public IndexBlazorComponent()
		{
		}

		protected override void OnDataContextAssigned(BaseVm dataContext)
		{
			base.OnDataContextAssigned(dataContext);

			if (dataContext is IVisualizationRequestVm vm)
				vm.VisualizationRequest = this.VisualizationRequest;
		}

		protected override Task OnInitializedAsync()
		{
			var vizRequest = GetObjectFromQueryString<CrossModuleVisualizationRequest>("request");
			this.VisualizationRequest = vizRequest;

			return base.OnInitializedAsync();
		}
	}
}
