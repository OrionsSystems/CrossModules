using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.CrossModules.Blazor
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
