using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Orions.Systems.CrossModules.Desi.Infrastructure
{
	public static class JsRuntimeExtensions
	{
		public static async Task<T> InvokeAsyncWithPromise<T>(this IJSRuntime jSRuntime, string jsFunctionName, params object[] args)
		{
			var promiseHandler = new JsPromiseHandler<T>(jSRuntime);

			var result = await promiseHandler.CallAndWaitForResult(jsFunctionName, args);

			return result;
		}

		public static async Task InvokeVoidAsyncWithPromise(this IJSRuntime jSRuntime, string jsFunctionName, params object[] args)
		{
			var promiseHandler = new JsPromiseHandler(jSRuntime);

			await promiseHandler.CallAndWaitForResult(jsFunctionName, args);
		}
	}

	public class JsPromiseHandler<TResult>
	{
		protected readonly IJSRuntime jSRuntime;
		protected TaskCompletionSource<TResult> _resultTaskCompletionSource = new TaskCompletionSource<TResult>();
		protected DotNetObjectReference<JsPromiseHandler<TResult>> _handlerJsReference;

		public JsPromiseHandler(IJSRuntime jSRuntime)
		{
			this.jSRuntime = jSRuntime;
			_handlerJsReference = DotNetObjectReference.Create(this);
		}

		[JSInvokable]
		public void ResolvePromise(TResult result)
		{
			_resultTaskCompletionSource.SetResult(result);
		}

		[JSInvokable]
		public void ErrorPromise(string exception)
		{
			_resultTaskCompletionSource.SetException(new Exception(exception));
		}

		public async Task<TResult> CallAndWaitForResult(string jsFunctionName, object[] args)
		{
			await jSRuntime.InvokeVoidAsync("Orions.JsInteropUtils.callFunctionWithPromise", _handlerJsReference, jsFunctionName, args);

			var result = await _resultTaskCompletionSource.Task;

			return result;
		}
	}

	public class JsPromiseHandler : JsPromiseHandler<bool>
	{
		public JsPromiseHandler(IJSRuntime jSRuntime) : base(jSRuntime)
		{
		}

		[JSInvokable]
		public void ResolvePromiseVoid()
		{
			_resultTaskCompletionSource.SetResult(true);
		}
	}
}
