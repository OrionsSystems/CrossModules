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

		BaseVm _parentVm = null;
		public BaseVm ParentVm
		{
			get => _parentVm;
			set
			{
				_parentVm = value;
				OnSetParentVm(value);
			}
		}

		public BlazorVm()
		{
		}

		protected virtual void OnSetParentVm(BaseVm vm)
		{
		}
	}
}
