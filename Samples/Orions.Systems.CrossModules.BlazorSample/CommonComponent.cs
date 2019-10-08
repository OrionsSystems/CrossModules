using Microsoft.AspNetCore.Components;
using Orions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.BlazorSample;
using Orions.Systems.CrossModules.Blazor;

namespace Orions.Systems.CrossModules.BlazorSample
{
	public class CommonComponent<VmType> : BaseOrionsComponent
		where VmType : BaseVm
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

		public CommonComponent()
		{
			// TODO: remove
			object oo = new TestVm();
			DataContext = (VmType)oo;
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
