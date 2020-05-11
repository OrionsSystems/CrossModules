using Microsoft.AspNetCore.Components;

using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class TagonomyEditorBase : BaseBlazorComponent<TagonomyEditorVm>, IDisposable
	{
		IDisposable thisReference;

		[Parameter]
		public IHyperArgsSink HyperStore
		{
			get => Vm.HyperStore;
			set => Vm.HyperStore = value;
		}

		[Parameter]
		public string TagonomyId
		{
			get => Vm.TagonomyId;
			set => Vm.TagonomyId = value;
		}

		public LoaderConfiguration LoaderSetting { get; set; } = new LoaderConfiguration() { Visible = true };

		public TagonomyEditorBase()
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
