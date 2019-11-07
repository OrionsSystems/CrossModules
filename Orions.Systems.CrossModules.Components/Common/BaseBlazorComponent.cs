using Microsoft.AspNetCore.Components;
using Orions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class BaseBlazorComponent : BaseOrionsComponent
	{
	}

	public class BaseBlazorComponent<VmType> : BaseBlazorComponent
		where VmType : BaseVm, new()
	{
		VmType _dataContext = null;

		[Parameter]
		public VmType DataContext
		{
			get
			{
				return _dataContext;
			}

			set
			{
				if (_dataContext != null)
				{
					_dataContext.PropertyChanged -= DataContext_PropertyChanged;
					if (_dataContext is BlazorVm blazorVmPrevious)
						blazorVmPrevious.OwnerComponent = null;
				}

				_dataContext = value;
				if (value != null)
				{
					value.PropertyChanged += DataContext_PropertyChanged;
				}

				if (value is BlazorVm blazorVm)
					blazorVm.OwnerComponent = this;
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
