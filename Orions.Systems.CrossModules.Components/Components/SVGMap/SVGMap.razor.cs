using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class SVGMapBase : BaseOrionsComponent, IDisposable
	{
		private string _hideElementCSS = "hide-zone";
		private string _classNameOnMouseOver = "zone-over";
		private string _zoneAreaId = "Zones";

		private IDisposable thisReference;

		public string ClickedZone { get; set; }

		[Parameter]
		public string Zone { get; set; }

		[Parameter]
		public EventCallback<string> OnClickZone { get; set; }

		protected override async Task OnFirstAfterRenderAsync()
		{
			thisReference = DotNetObjectReference.Create(this);

			await JsInterop.InvokeAsync<object>("Orions.MapZone.Init", new object[] { _zoneAreaId, _classNameOnMouseOver, thisReference });

			if (!string.IsNullOrEmpty(Zone))
			{
				var zones = Zone.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(it => it.Trim()).ToArray();

				foreach (var zone in zones) {
					await JsInterop.InvokeAsync<object>("Orions.MapZone.RemoveClassById", new object[] { zone, _hideElementCSS });
				}
			}

			await base.OnInitializedAsync();
		}

		public async Task UpdateZone(string oldZone, string newZone) {
			if (!string.IsNullOrWhiteSpace(oldZone)) {
				await JsInterop.InvokeAsync<object>("Orions.MapZone.AddClassById", new object[] { oldZone, _hideElementCSS });
			}

			if (!string.IsNullOrWhiteSpace(newZone))
			{
				await JsInterop.InvokeAsync<object>("Orions.MapZone.RemoveClassById", new object[] { newZone, _hideElementCSS });
			}
		}

		[JSInvokable]
		public Task OnZoneClick(string zone)
		{
			return OnClickZone.InvokeAsync(zone);
		}

		void IDisposable.Dispose()
		{
			thisReference?.Dispose();
		}
	}
}
