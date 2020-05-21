using Microsoft.AspNetCore.Components;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;
using Syncfusion.EJ2.Blazor.Navigations;
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

		[Parameter]
		public EventCallback<Tagonomy> OnShowVizList
		{
			get => Vm.OnShowVizList;
			set => Vm.OnShowVizList = value;
		}

		public LoaderConfiguration LoaderSetting { get; set; } = new LoaderConfiguration() { Visible = true };

		public TagonomyEditorBase()
		{

		}

		public void OnSelect(NodeSelectEventArgs args)
		{
			Vm.OnSelect(args);
			StateHasChanged();
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
