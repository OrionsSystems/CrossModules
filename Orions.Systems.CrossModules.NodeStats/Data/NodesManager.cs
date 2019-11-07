using System;
using System.Linq;
using System.Threading.Tasks;

using Orions.Common;
using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossModules.NodeStats
{
	public class NodesManager : NotifyPropertyChanged, IDisposable
	{
		public static NodesManager Instance { get; } = new NodesManager();

		static NodesManager()
		{
		}

		readonly HotSwapListLite<NodeDataManager> _nodes = new HotSwapListLite<NodeDataManager>();

		public NodeDataManager[] Nodes => _nodes.ToArray();

		bool Disposed { get; set; }

		public NodesManager()
		{
		}

		public void Dispose()
		{
			Disposed = true;
		}

		public async Task InitAsync(HyperConnectionSettings[] connections)
		{
			if (connections != null)
			{
				foreach (var connection in connections)
				{
					await AddNodeAsync(connection.Alias, connection.ConnectionUri);
				}
			}

			var t = Task.Factory.StartNew(UpdateWorker);
		}

		public async Task AddNodeAsync(string name, string url)
		{
			var manager = new NodeDataManager();
			await manager.InitAsync(name, url);
			manager.PropertyChanged += Manager_PropertyChanged;
			_nodes.Add(manager);
		}

		private void Manager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Notify("Manager." + e.PropertyName);
		}

		public async Task UpdateWorker()
		{
			while (this.Disposed == false && ApplicationLifetimeHelper.IsApplicationClosing == false)
			{
				await Task.Delay(2000);

				foreach (var node in _nodes)
				{
					try
					{
						await node.RunUpdatesAsync();
					}
					catch
					{
						// ignored
					}
				}
			}
		}
	}
}
