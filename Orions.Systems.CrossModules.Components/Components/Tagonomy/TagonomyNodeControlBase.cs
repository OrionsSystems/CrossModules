using Microsoft.AspNetCore.Components;

using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orions.Infrastructure.HyperSemantic;

namespace Orions.Systems.CrossModules.Components
{
	public class TagonomyNodeControlBase : BaseBlazorComponent<TagonomyNodeControlVm>, IDisposable
	{
		IDisposable thisReference;

		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => Vm.HyperStore;
			set => Vm.HyperStore = value;
		}

		[Parameter]
		public TagonomyNode SelectedTagonomyNode
		{
			get => Vm.Node; 
			set => Vm.Node = value;
		}

		public TagonomyNodeControlBase()
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
