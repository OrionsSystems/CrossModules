using Blazored.LocalStorage;
using Microsoft.JSInterop;
using Orions.Desi.Forms.Core.Services;
using Orions.Systems.Desi.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Debug.Infrastructure
{
	public class BlazorDependencyResolver : DependencyResolver, IDependencyResolver
	{
		private readonly ILocalStorageService _localStorageService;

		public BlazorDependencyResolver(ILocalStorageService localStorageService)
		{
			_localStorageService = localStorageService;
		}

		public override IApiHelper GetApiHelper()
		{
			return null;
			throw new NotImplementedException();
		}

		public override IDialogService GetDialogService()
		{
			return null;
			throw new NotImplementedException();
		}

		public override IImageService GetImageService()
		{
			return null;
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
	}
}
