using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
	public class BaseTabItem : BaseBlazorComponent, IDisposable
	{
		[Parameter] public string Name { get; set; }

		[Parameter] public string Classes { get; set; }

		[Parameter] public bool Enable { get; set; } = true;

		[Parameter] public bool Active { get { return _data.Active; } set { _data.Active = value; } }

		[Parameter] public bool Action { get; set; }

		[Parameter] public string OutTransitions { get; set; }

		[Parameter] public string InTransitions { get; set; }

		public string AnimationClass { get { return _data.AnimationClass; } }

		// Each time the params change, update a 'TabContainerData' instance
		private readonly TabContainerData _data = new TabContainerData();

		protected override void OnParametersSet()
		{
			_data.Name = Name;
			_data.Class = Classes;
			_data.Enable = Enable;
			_data.Action = Action;
			_data.OutTransitions = OutTransitions;
			_data.InTransitions = InTransitions;

		}

		// When we're first added to the UI, attach our data to parent
		// When we're removed from the UI, remove our data from parent
		[CascadingParameter] public TabContainer Container { get; set; }

		[Parameter]
		public RenderFragment ChildContent { get; set; }

		protected override void OnInitialized() 
		{
			Container.Items.Add(_data);

			if(Container.Items.Count == 1)
				_data.Active = true;

			StateHasChanged();
		}

		void IDisposable.Dispose()
			 => Container.Items.Remove(_data);
	}
}
