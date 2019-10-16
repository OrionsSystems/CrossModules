using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace Orions.Systems.CrossModules.Blazor
{
	/// <summary>
	/// Introduction to Blazor components
	/// https://docs.microsoft.com/en-us/aspnet/core/blazor/components?view=aspnetcore-3.0
	/// </summary>
	public class BaseOrionsComponent : ComponentBase
	{
		ElementReference _ref;

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

		[Inject]
		protected NavigationManager UriHelper { get; set; }


		[Inject]
		protected IJSRuntime JsInterop { get; set; }

		public BaseOrionsComponent()
		{
		}

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

		public string GetQueryParameterString(string queryParameter)
		{
			var uri = new Uri(UriHelper.Uri);
			var stringResult = QueryHelpers.ParseQuery(uri.Query).TryGetValue(queryParameter, out var paramValue) ? paramValue.First() : "";

			return stringResult;
		}

		public TType GetObjectFromQueryString<TType>(string queryParameter)
		{
			TType objectType = default(TType);

			var stringResult = GetQueryParameterString(queryParameter);
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
