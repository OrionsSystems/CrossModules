using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace Orions.Systems.CrossModules.Portal.Providers
{
	public class CustomSettingsProvider
	{
		private ILocalStorageService _storage;

		public static string THEME_KEY = "t_k";

		public static string THEME_DARK = "Dark";
		public static string THEME_WHITE = "White";

		public CustomSettingsProvider(ILocalStorageService storage)
		{
			_storage = storage;
		}

		public async Task SetParameter(string key, string value) {
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException(nameof(key));

			if (string.IsNullOrWhiteSpace(value))
				throw new ArgumentException(nameof(value));

			await _storage.SetItemAsync(key, value);
		}

		public async Task<string> GetParameter(string key) 
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException(nameof(key));

			return await _storage.GetItemAsync<string>(key);
		}

	}
}