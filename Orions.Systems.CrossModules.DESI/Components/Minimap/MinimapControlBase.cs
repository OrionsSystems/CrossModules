using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Extensions;
using Orions.Systems.Desi.Common.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Components.Minimap
{
	public class MinimapControlBase : BaseComponent
	{
		private bool _initialized = false;
		private IMediaDataStore _mediaDataStore;

		[Inject]
		public IMediaDataStore MediaDataStore
		{
			get
			{
				return _mediaDataStore;
			}
			set
			{
				SetProperty(ref _mediaDataStore, value, () =>
				{
					CleanupDataStoreSubscriptions();

					_dataStoreSubscriptions.Add(_mediaDataStore.Data.GetPropertyChangedObservable()
						.Where(e => e.EventArgs.PropertyName == nameof(MediaData.DefaultMediaInstance))
						.Subscribe(_ => {
							_mediaDataStore.Data.DefaultMediaInstance?.GetPropertyChangedObservable()
								.Where(e => e.EventArgs.PropertyName == nameof(MediaInstance.CurrentPositionFrameImage))
								.Subscribe(_ => UpdateState());

							UpdateState();
						}));

					_dataStoreSubscriptions.Add(_mediaDataStore.Data.DefaultMediaInstance?.GetPropertyChangedObservable()
						.Where(e => e.EventArgs.PropertyName == nameof(MediaInstance.CurrentPositionFrameImage))
						.Subscribe(_ => UpdateState()));
				});
			}
		}

		public byte[] CurrentFrameImage
		{
			get
			{
				return MediaDataStore.Data?.DefaultMediaInstance?.CurrentPositionFrameImage;
			}
		}

		private object _initLock = new object();
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
			JSRuntime.InvokeVoidAsync("Orions.Minimap.dispose");

			base.Dispose(disposing);
		}
	}
}
