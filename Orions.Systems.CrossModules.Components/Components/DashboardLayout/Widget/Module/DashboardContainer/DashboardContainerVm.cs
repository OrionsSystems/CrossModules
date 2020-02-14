using System;
using System.Linq;
using System.Threading.Tasks;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(DashboardContainerWidget))]
	public class DashboardContainerVm : WidgetVm<DashboardContainerWidget>
	{		
		public DashboardContainerVm()
		{
		}

		public string ParrentDashboardId { get { return this.Widget.Dashboard?.Id; } }

		public async Task<DashboardData> SetDashboardAsync()
		{
			if (string.IsNullOrWhiteSpace(this.Widget.Dashboard?.Id))
			{
				return null;
			}

			var documentId = HyperDocumentId.Create<DashboardData>(this.Widget.Dashboard?.Id);
			var args = new RetrieveHyperDocumentArgs(documentId);
			var doc = await HyperStore.ExecuteAsync(args);

			if (args.ExecutionResult.IsNotSuccess)
				return null;

			var dashboard = doc?.GetPayload<DashboardData>();

			if (dashboard == null)
				return null;

			(this.ParentVm as DashboardVm).Source = dashboard;

			return dashboard;
		}
	}
}
