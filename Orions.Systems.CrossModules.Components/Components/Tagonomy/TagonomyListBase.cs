using Microsoft.AspNetCore.Components;

using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;

using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class TagonomyListBase : BaseBlazorComponent<TagonomyListVm>, IDisposable
	{
		IDisposable thisReference;

		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => Vm.HyperStore;
			set => Vm.HyperStore = value;
		}

		[Parameter]
		public EventCallback<Tagonomy> OnManage
		{
			get => Vm.OnManage;
			set => Vm.OnManage = value;
		}

		[Parameter]
		public EventCallback<Tagonomy> OnEdit
		{
			get => Vm.OnEdit;
			set => Vm.OnEdit = value;
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

		public TagonomyListBase()
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
