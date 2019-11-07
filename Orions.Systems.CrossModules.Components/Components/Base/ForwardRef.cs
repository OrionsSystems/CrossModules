using Microsoft.AspNetCore.Components;

namespace Orions.Systems.CrossModules.Components
{
	public class ForwardRef
	{
		private ElementReference _current;

		public ElementReference Current
		{
			get => _current;
			set => Set(value);
		}


		public void Set(ElementReference value)
		{
			_current = value;
		}
	}
}
