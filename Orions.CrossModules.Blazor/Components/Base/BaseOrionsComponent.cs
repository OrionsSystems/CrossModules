using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Orions.Infrastructure.HyperMedia;
using Microsoft.AspNetCore.WebUtilities;

namespace Orions.CrossModules.Blazor
{
	public class BaseOrionsComponent : ComponentBase
	{

		private bool isRendered = false;

		private ElementReference _ref;

		/// <summary>
		/// Returned ElementRef reference for DOM element.
		/// </summary>
		[Parameter]
		public virtual ElementReference Ref
		{
			get => _ref;
			set
			{
				_ref = value;
				RefBack?.Set(value);
			}
		}

		[Parameter]
		public ForwardRef RefBack { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await OnFirstAfterRenderAsync();
			}

			await base.OnAfterRenderAsync(firstRender);
		}


		protected virtual Task OnFirstAfterRenderAsync()
		{
			return Task.CompletedTask;
		}

		[Inject]
		protected NavigationManager UriHelper { get; set; }


		[Inject]
		protected IJSRuntime JsInterop { get; set; }


		protected TType GetObjectFromQueryString<TType>(string queryParameter)
		{
			TType objectType = default(TType);

			var uri = new Uri(UriHelper.Uri);
			var stringResult = QueryHelpers.ParseQuery(uri.Query).TryGetValue(queryParameter, out var paramValue) ? paramValue.First() : "";
			if (!string.IsNullOrEmpty(stringResult))
			{
				try
				{
					objectType = Orions.Common.JsonHelper.Deserialize<TType>(stringResult);
					return objectType;
				}
				catch (Exception)
				{

					return objectType;
				}
			}
			return objectType;
		}

		#region JS interoperability
		/// <summary>
		/// Hack to fix https://github.com/aspnet/AspNetCore/issues/11159
		/// </summary>
		public static object CreateDotNetObjectRefSyncObj = new object();

		/// <summary>
		/// To invoke a .NET instance method from JavaScript
		/// </summary>
		/// <example>https://docs.microsoft.com/en-us/aspnet/core/blazor/javascript-interop?view=aspnetcore-3.0 </example>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		protected DotNetObjectReference<T> CreateDotNetObjectRef<T>(T value) where T : class
		{
			lock (CreateDotNetObjectRefSyncObj)
			{
				//JSRuntime.SetCurrentJSRuntime(JsInterop);
				return DotNetObjectReference.Create(value);
			}
		}

		protected void DisposeDotNetObjectRef<T>(DotNetObjectReference<T> value) where T : class
		{
			if (value != null)
			{
				lock (CreateDotNetObjectRefSyncObj)
				{
					//JSRuntime.SetCurrentJSRuntime(JsInterop);
					value.Dispose();
				}
			}
		}

		#endregion
	}
}
