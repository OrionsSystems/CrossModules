using Microsoft.AspNetCore.Components;

using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class BlazorVm : BaseVm, IBlazorVm
	{
		BaseBlazorComponent _ownerComponent = null;

		/// <summary>
		/// The Blazor Component that owns and operates this View model.
		/// </summary>
		public BaseBlazorComponent OwnerComponent
		{
			get
			{
				return _ownerComponent;
			}

			set
			{
				SetValue(ref _ownerComponent, value);
			}
		}

		public EventCallback<ToastMessage> OnToastMessage { get; set; }

		public BlazorVm()
		{
		}
	}
}
