﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class SVGMapBase : BaseOrionsComponent, IDisposable
	{
		private string _removeElementCSS = "hide-zone";
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
				await JsInterop.InvokeAsync<object>("Orions.MapZone.RemoveClassById", new object[] { Zone, _removeElementCSS });
			}

			await base.OnInitializedAsync();
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
