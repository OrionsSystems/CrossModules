using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Orions.Systems.Desi.Common.Services;
using System;
using System.Collections.Generic;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Infrastructure
{
	public abstract class BaseComponent : ComponentBase, IDisposable
	{
		protected List<IDisposable> _dataStoreSubscriptions = new List<IDisposable>();

		[Inject]
		public BlazorDependencyResolver DependencyResolver { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		[Inject]
		public ILoggerService Logger { get; set; }

		#region Lifecycle methods overrides
		protected sealed override void OnAfterRender(bool firstRender)
		{
			try
			{
				this.OnAfterRenderSafe(firstRender);
				base.OnAfterRender(firstRender);
			}
			catch (Exception e)
			{
				LogException(e);
				throw;
			}
		}

		protected virtual void OnAfterRenderSafe(bool firstRender)
		{
		}

		protected sealed override async Task OnAfterRenderAsync(bool firstRender)
		{
			try
			{
				await this.OnAfterRenderAsyncSafe(firstRender);
				await base.OnAfterRenderAsync(firstRender);
			}
			catch (Exception e)
			{
				LogException(e);
				throw;
			}
		}

		protected virtual Task OnAfterRenderAsyncSafe(bool firstRender)
		{
			return Task.CompletedTask;
		}

		protected sealed override void OnInitialized()
		{
			try
			{
				this.OnInitializedSafe();
				base.OnInitialized();
			}
			catch (Exception e)
			{
				LogException(e);
				throw;
			}
		}

		protected virtual void OnInitializedSafe()
		{
		}

		protected sealed override async Task OnInitializedAsync()
		{
			try
			{
				await this.OnInitializedAsyncSafe();
				await base.OnInitializedAsync();
			}
			catch (Exception e)
			{
				LogException(e);
				throw;
			}
		}

		protected virtual Task OnInitializedAsyncSafe()
		{
			return Task.CompletedTask;
		}

		protected sealed override void OnParametersSet()
		{
			try
			{
				this.OnParametersSetSafe();
				base.OnParametersSet();
			}
			catch(Exception e)
			{
				LogException(e);
				throw;
			}
		}

		protected virtual void OnParametersSetSafe()
		{
		}

		protected sealed override async Task OnParametersSetAsync()
		{
			try
			{
				await this.OnParametersSetAsyncSafe();
				await base.OnParametersSetAsync();
			}
			catch (Exception e)
			{
				LogException(e);
				throw;
			}
		}

		protected virtual Task OnParametersSetAsyncSafe()
		{
			return Task.CompletedTask;
		}

		protected sealed override bool ShouldRender()
		{
			try
			{
				return this.ShouldRenderSafe();
			}
			catch(Exception e)
			{
				LogException(e);
				throw;
			}
		}

		protected virtual bool ShouldRenderSafe()
		{
			return base.ShouldRender();
		}
		#endregion

		private void LogException(Exception e)
		{
			try
			{
				Logger.LogException("Exception", e);
			}
			catch { }
		}

		protected bool SetProperty<T>(ref T storage,
			T value,
			Action onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(storage, value))
				return false;
			storage = value;
			onChanged?.Invoke();
			return true;
		}

		public void UpdateState()
		{
			if (!IsDisposed)
			{
				InvokeAsync(StateHasChanged);
			}
		}

		#region IDisposable Support
		protected bool IsDisposed = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				CleanupDataStoreSubscriptions();

				IsDisposed = true;
			}
		}

		private void CleanupDataStoreSubscriptions()
		{
			foreach(var sub in _dataStoreSubscriptions)
			{
				sub.Dispose();
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~DesiBaseComponent()
		// {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
