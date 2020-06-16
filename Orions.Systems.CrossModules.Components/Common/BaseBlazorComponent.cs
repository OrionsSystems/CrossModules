using Microsoft.AspNetCore.Components;

using Orions.Common;

using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class BaseBlazorComponent<VmType> : BaseBlazorComponent
		where VmType : BlazorVm, new()
	{
		protected virtual bool AutoCreateVm => true;

		[Parameter]
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

		[Parameter]
		public EventCallback<ToastMessage> OnToastMessage
		{
			get => Vm.OnToastMessage;
			set => Vm.OnToastMessage = value;
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

		protected override void OnDataContextAssigned(object dataContext)
		{
			base.OnDataContextAssigned(dataContext);
		}
	}

	public class BaseBlazorComponent : BaseOrionsComponent
	{
		object _dataContext;

		[Parameter]
		public object DataContext
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

					var baseVm = (BaseVm)value;
					if (baseVm != null)
						baseVm.PropertyChanged -= DataContext_PropertyChanged;

					if (_dataContext is BlazorVm blazorVmPrevious)
						blazorVmPrevious.OwnerComponent = null;
				}

				_dataContext = value;
				if (value != null)
				{
					var baseVm = (BaseVm)_dataContext;
					if (baseVm != null)
						baseVm.PropertyChanged += DataContext_PropertyChanged;
				}

				OnDataContextAssigned(value);
			}
		}

		[Parameter]
		public BaseVm ParentVm
		{
			get => (_dataContext as BlazorVm)?.ParentVm;
			set => ((BlazorVm)_dataContext).ParentVm = value;
		}

		bool _initialized = false;

		public BaseBlazorComponent()
		{
		}

		protected virtual void OnDataContextAssigned(object dataContext)
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
