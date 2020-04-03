using Blazored.LocalStorage;
using Microsoft.JSInterop;
using Orions.Desi.Forms.Core.Services;
using Orions.Systems.Desi.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.CrossModules.Desi.Services;
using Microsoft.Extensions.Configuration;

namespace Orions.Systems.CrossModules.Desi.Infrastructure
{
	public class BlazorDependencyResolver : DependencyResolver, IDependencyResolver
	{
		private readonly ILocalStorageService _localStorageService;
		private readonly NavigationManager _navigationManager;
		private readonly IJSRuntime _jSRuntime;
		private readonly IConfiguration _appConfig;
		private PopupService _popupService;

		public BlazorDependencyResolver(ILocalStorageService localStorageService, NavigationManager navigationManager, IJSRuntime jSRuntime, IConfiguration appConfig)
		{
			_localStorageService = localStorageService;
			_navigationManager = navigationManager;
			_jSRuntime = jSRuntime;
			_appConfig = appConfig;
		}

		public override IApiHelper GetApiHelper()
		{
			return new WebApiHelper();
		}

		public override IDialogService GetDialogService()
		{
			return new BlazorDialogService(_popupService);
		}

		public override IImageService GetImageService()
		{
			return new BlazorImageService();
		}

		public override IDispatcher GetDispatcher()
		{
			return new BlazorDispatcher();
		}

		private ISettingsStorage _settingsStorage;

		public override ISettingsStorage GetSettingsStorage()
		{
			if (_settingsStorage == null)
			{
				_settingsStorage = new BrowserLocalSettingsStorage(_localStorageService, _appConfig);
			}

			return _settingsStorage;
		}

		public override IRecognitionService GetRecognitionService()
		{
			return null;
			throw new NotImplementedException();
		}

		public override ISegmentationConfigStorage GetSegmentationConfigStorage()
		{
			return null;
			throw new NotImplementedException();
		}

		public override IDeviceClipboardService GetDeviceClipboardService()
		{
			return null;
			throw new NotImplementedException();
		}

		public override ILoggerService GetLoggerService()
		{
			return new BlazorLoggerService();
		}

		public INavigationService GetNavigationService()
		{
			return new NavigationService(_navigationManager, _jSRuntime, _popupService);
		}

		public void SetPopupService(PopupService service)
		{
			this._popupService = service;
		}

		public PopupService GetPopupService()
		{
			return this._popupService;
		}
	}
}
