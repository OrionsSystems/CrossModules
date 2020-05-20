using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.Media;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Orions.Systems.CrossModules.Desi.Components.Minimap
{
	public class MinimapControlBase : BaseComponent
	{
		private bool _initialized = false;
		private IDisposable _currentPositionChangedSub;

		[Inject]
		public IMediaDataStore MediaDataStore { get; set; }

		public byte[] CurrentFrameImage
		{
			get
			{
				return MediaDataStore.Data?.DefaultMediaInstance?.CurrentPositionFrameImage;
			}
		}

		private object _initLock = new object();

		protected override void OnInitializedSafe()
		{
			base.OnInitializedSafe();

			_dataStoreSubscriptions.Add(MediaDataStore.Data.GetPropertyChangedObservable()
				.Where(e => e.EventArgs.PropertyName == nameof(MediaData.DefaultMediaInstance))
				.Select(i => i.Source.DefaultMediaInstance)
				.Subscribe(instance =>
				{
					_currentPositionChangedSub?.Dispose();
					_currentPositionChangedSub = instance?.GetPropertyChangedObservable()
						.Where(e => e.EventArgs.PropertyName == nameof(MediaInstance.CurrentPositionFrameImage))
						.Subscribe(_ => UpdateState());
					UpdateState();
				}));

			_currentPositionChangedSub = MediaDataStore.Data.DefaultMediaInstance?.GetPropertyChangedObservable()
				.Where(e => e.EventArgs.PropertyName == nameof(MediaInstance.CurrentPositionFrameImage))
				.Subscribe(_ => UpdateState());
		}

		protected override void OnAfterRenderSafe(bool firstRender)
		{
			lock(_initLock)
			{
				if (!_initialized)
				{
					_initialized = true;
					JSRuntime.InvokeVoidAsync("Orions.Minimap.init");
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_currentPositionChangedSub?.Dispose();
				JSRuntime.InvokeVoidAsync("Orions.Minimap.dispose");
			}

			base.Dispose(disposing);
		}
	}
}
