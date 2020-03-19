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

namespace Orions.Systems.CrossModules.Desi.Infrastructure
{
	public class BlazorDependencyResolver : DependencyResolver, IDependencyResolver
	{
		private readonly ILocalStorageService _localStorageService;
		private readonly NavigationManager _navigationManager;
		private readonly IJSRuntime _jSRuntime;

		public BlazorDependencyResolver(ILocalStorageService localStorageService, NavigationManager navigationManager, IJSRuntime jSRuntime)
		{
			_localStorageService = localStorageService;
			_navigationManager = navigationManager;
			_jSRuntime = jSRuntime;
		}

		public override IApiHelper GetApiHelper()
		{
			return new WebApiHelper();
		}

		public override IDialogService GetDialogService()
		{
			return null;
			throw new NotImplementedException();
		}

		public override IImageService GetImageService()
		{
			return new BlazorImageService();
			throw new NotImplementedException();
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
				_settingsStorage = new BrowserLocalSettingsStorage(_localStorageService);
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
			return null;
			throw new NotImplementedException();
		}

		public INavigationService GetNavigationService()
		{
			return new NavigationService(_navigationManager, _jSRuntime);
		}
	}
}
