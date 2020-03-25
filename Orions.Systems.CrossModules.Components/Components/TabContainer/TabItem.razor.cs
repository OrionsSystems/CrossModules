using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
	public class BaseTabItem : BaseBlazorComponent, IDisposable
	{
		[Parameter] public string Name { get; set; }

		[Parameter] public bool Enable { get; set; } = true;

		[Parameter] public bool Active { get { return data.Active; } set { data.Active = value; } }

		[Parameter] public bool Action { get; set; }

		// Each time the params change, update a 'TabContainerData' instance
		private readonly TabContainerData data = new TabContainerData();

		protected override void OnParametersSet()
		{
			data.Name = Name;
			data.Enable = Enable;
			data.Action = Action;
		}

		// When we're first added to the UI, attach our data to parent
		// When we're removed from the UI, remove our data from parent
		[CascadingParameter] public TabContainer Container { get; set; }

		[Parameter]
		public RenderFragment ChildContent { get; set; }

		protected override void OnInitialized() 
		{
			Container.Items.Add(data);

			if(Container.Items.Count == 1)
				data.Active = true;

			StateHasChanged();
		}

		void IDisposable.Dispose()
			 => Container.Items.Remove(data);
	}
}
