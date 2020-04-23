using Microsoft.AspNetCore.Components;

using Orions.Node.Common;

using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class MissionListBase : BaseBlazorComponent<MissionListVm>, IDisposable
	{
		IDisposable thisReference;

		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => Vm.HyperStore;
			set => Vm.HyperStore = value;
		}


		PropertyGrid _propGrid;
		PropertyGrid propGrid
		{
			get => _propGrid;
			set
			{
				_propGrid = value;
				Vm.PropertyGridVm = value.Vm;
			}
		}

		public LoaderConfiguration LoaderSetting { get; set; } = new LoaderConfiguration() { Visible = true };

		public MissionListBase()
		{

		}

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			await Vm.Init();
		}

		void IDisposable.Dispose()
		{
			thisReference?.Dispose();
		}
	}
}
