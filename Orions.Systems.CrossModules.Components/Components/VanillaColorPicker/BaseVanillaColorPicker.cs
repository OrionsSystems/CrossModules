using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class BaseVanillaColorPicker : BaseOrionsComponent
	{
		[Parameter]
		public string ParentId { get; set; }

		[Parameter]
		public string Label { get; set; }

		protected override async Task OnFirstAfterRenderAsync()
		{
			if (string.IsNullOrWhiteSpace(ParentId)) throw new ArgumentException();

			await JsInterop.InvokeAsync<object>("Orions.VanillaColorPicker.init", new object[] { ParentId });
		}
	}
}
