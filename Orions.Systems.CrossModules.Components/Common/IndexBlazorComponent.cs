using Orions.Common;
using Orions.Infrastructure.HyperMedia;

using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class IndexBlazorComponent<VmType> : BaseBlazorComponent<VmType>
		where VmType : BlazorVm, new()
	{
		public CrossModuleVisualizationRequest VisualizationRequest { get; set; }

		public IndexBlazorComponent()
		{
		}

		protected override void OnDataContextAssigned(object dataContext)
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
