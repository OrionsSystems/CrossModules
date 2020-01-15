using Microsoft.AspNetCore.Components;
using Orions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class BaseBlazorComponent<VmType> : BaseBlazorComponent
		where VmType : BaseVm, new()
	{
		protected virtual bool AutoCreateVm => true;

		public VmType Vm
		{
			get
			{
				return (VmType)base.DataContext;
			}

			set
			{
				base.DataContext = value;
			}
		}

		[Obsolete("Use Vm instead")]
		public new VmType DataContext
		{
			get
			{
				return Vm;
			}

			set
			{
				base.DataContext = value;
			}
		}

		public BaseBlazorComponent()
		{
			if (this.AutoCreateVm)
			{
				base.DataContext = new VmType();
			}
		}

		protected override void OnDataContextAssigned(BaseVm dataContext)
		{
			base.OnDataContextAssigned(dataContext);
		}
	}

	public class BaseBlazorComponent : BaseOrionsComponent
	{
		BaseVm _dataContext;

		[Parameter]
		public BaseVm DataContext
		{
			get
			{
				return _dataContext;
			}

			set
			{
				if (_dataContext == value)
					return;

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

				OnDataContextAssigned(value);
			}
		}

		[Parameter]
		public BaseVm ParentVm
		{
			get => _dataContext?.ParentVm;
			set => _dataContext.ParentVm = value;
		}

		bool _initialized = false;

		public BaseBlazorComponent()
		{
		}

		protected virtual void OnDataContextAssigned(BaseVm dataContext)
		{
			if (dataContext is IBlazorVm blazorVm)
				blazorVm.OwnerComponent = this;
		}

		void DataContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (_initialized == false)
				return;

			var t = this.InvokeAsync(() =>
			{
				OnStateHasChanged();
			});
		}

		protected virtual void OnStateHasChanged()
		{
			this.StateHasChanged();
		}

		protected override Task OnInitializedAsync()
		{
			_initialized = true;
			return base.OnInitializedAsync();
		}
	}
}
