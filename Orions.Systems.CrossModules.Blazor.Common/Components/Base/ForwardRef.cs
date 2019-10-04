using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Blazor.Common.Components
{
	public class ForwardRef
	{
		private ElementRef _current;

		public ElementRef Current
		{
			get => _current;
			set => Set(value);
		}


		public void Set(ElementRef value)
		{
			_current = value;
		}
	}
}
