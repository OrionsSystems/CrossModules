using Microsoft.AspNetCore.Components;
using Orions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Blazor
{
	public class BaseBlazorComponent : BaseOrionsComponent
	{
	}

	public class BaseBlazorComponent<VmType> : BaseBlazorComponent
		where VmType : BaseVm, new()
	{
		VmType _dataContext = null;

		public VmType DataContext
		{
			get
			{
				return _dataContext;
			}

			set
			{
				_dataContext = value;
				if (value != null)
				{
					value.PropertyChanged += DataContext_PropertyChanged;
				}
			}
		}

		bool _initialized = false;

		public BaseBlazorComponent()
		{
			// Create a default Vm instance.
			DataContext = new VmType();
		}

		void DataContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (_initialized == false)
				return;

			var t = this.InvokeAsync(() =>
			{
				this.StateHasChanged();
			});
		}

		protected override Task OnInitializedAsync()
		{
			_initialized = true;
			return base.OnInitializedAsync();
		}
	}
}
