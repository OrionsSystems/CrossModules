using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(AvailableNodesWidget))]
	public class AvailableNodesWidgetVm: WidgetVm<AvailableNodesWidget>
	{
		private HyperDomainHelper _hyperDomainHelper;

		public ObservableCollection<HyperConnectionSettings> Nodes { get; set; } = new ObservableCollection<HyperConnectionSettings>(new ObservableCollection<HyperConnectionSettings>());

		public AvailableNodesWidgetVm()
		{
			
		}

		public async Task Initiailize()
		{
			_hyperDomainHelper = new HyperDomainHelper("andrei", "a9090xxx");
			await _hyperDomainHelper.LoginToDomainAsyncThrows();

			var connections = (await _hyperDomainHelper.TryRetrieveAccessibleNodesAsync()).Result;

			Nodes.Clear();
			foreach(var conn in connections)
			{
				Nodes.Add(conn);
			}
		}
	}
}
