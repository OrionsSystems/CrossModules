using Orions.Common;

namespace Orions.Systems.CrossModules.Blazor
{
	public abstract class BlazorVm : BaseVm
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

		public BlazorVm()
		{
		}
	}
}
