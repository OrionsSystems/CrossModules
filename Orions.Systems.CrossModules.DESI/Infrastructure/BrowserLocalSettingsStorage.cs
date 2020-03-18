using Blazored.LocalStorage;
using Microsoft.JSInterop;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Orions.Desi.Forms.Core.Services.SettingsStorage;

namespace Orions.Systems.CrossModules.Desi.Infrastructure
{
	public class BrowserLocalSettingsStorage : ISettingsStorage
	{
		private readonly ILocalStorageService _localStorageService;
		private AppSettingsDto _appSettingsDto;
		private HyperNodeAuthenticationInfo _hyperNodeAuthenticationData;
		private HyperDomainAuthenticationInfo _hyperDomainAuthenticationInfo;

		public BrowserLocalSettingsStorage(ILocalStorageService localStorageService)
		{
			_localStorageService = localStorageService;
		}

		public string Token { get => _appSettingsDto.Token;  set => _appSettingsDto.Token = value;  }
		public bool IsStaySigned { get => _appSettingsDto.IsStaySigned; set => _appSettingsDto.IsStaySigned = value; }
		public bool IsDevModeEnabled { get => _appSettingsDto.IsDevModeEnabled; set => _appSettingsDto.IsDevModeEnabled = value; }
		public bool TaggingDisplayCrosshair { get => _appSettingsDto.TaggingDisplayCrosshair; set => _appSettingsDto.TaggingDisplayCrosshair = value; }

		public HyperNodeAuthenticationInfo HyperNodeAuthenticationData { get => _hyperNodeAuthenticationData; set => _hyperNodeAuthenticationData = value; }

		public HyperDomainAuthenticationInfo HyperDomainAuthenticationData { get => _hyperDomainAuthenticationInfo; set => _hyperDomainAuthenticationInfo = value; }

		public void AddCustomNode(HyperNodeAuthenticationInfo authenticationData)
		{
			_appSettingsDto.CustomNodes.Add(HyperNodeAuthenticationDataDto.CreateFromModel(authenticationData));
		}

		public IReadOnlyList<HyperNodeAuthenticationInfo> GetCustomNodes()
		{
			return _appSettingsDto.CustomNodes.Select(n => n.CreateModel()).ToList();
		}

		public void RemoveCustomNode(HyperNodeAuthenticationInfo authenticationData)
		{
			var node = _appSettingsDto.CustomNodes.SingleOrDefault(n => n.Id == authenticationData.Id);
			if (node != null)
			{
				_appSettingsDto.CustomNodes.Remove(node);
			}
		}

		public void UpdateCustomNode(HyperNodeAuthenticationInfo authenticationData)
		{
			var node = _appSettingsDto.CustomNodes.SingleOrDefault(n => n.Id == authenticationData.Id);
			if (node != null)
			{
				node.UpdateFromModel(authenticationData);
			}
		}

		public async Task Load()
		{
			var settings = await _localStorageService.GetItemAsync<AppSettingsDto>("desiSettings");

			if(settings != null)
			{
				_appSettingsDto = settings;
				_hyperDomainAuthenticationInfo = settings.HyperDomainAuthenticationData.CreateModel();
				_hyperNodeAuthenticationData = settings.HyperNodeAuthenticationData.CreateModel();
			}
			else
			{
				var nodeAuthData = new HyperNodeAuthenticationDataDto
				{
					Alias = HyperNodeAuthenticationInfo.DefaultAlias,
					ConnectionString = HyperNodeAuthenticationInfo.DefaultConnectionString,
					Id = Guid.NewGuid().ToString()
				};
				var domainAuthData = new HyperDomainAuthenticationDataDto();

				var dto = new AppSettingsDto
				{
					HyperNodeAuthenticationData = nodeAuthData,
					IsStaySigned = true,
					TaggingDisplayCrosshair = true,
					HyperDomainAuthenticationData = domainAuthData
				};
				dto.CustomNodes.Add(nodeAuthData);

				_hyperDomainAuthenticationInfo = dto.HyperDomainAuthenticationData.CreateModel();
				_hyperNodeAuthenticationData = dto.HyperNodeAuthenticationData.CreateModel();

				_appSettingsDto = dto;
			}
		}

		public async Task Save()
		{
			var settingsDto = new AppSettingsDto
			{
				Token = Token,
				IsDevModeEnabled = IsDevModeEnabled,
				IsStaySigned = IsStaySigned,
				TaggingDisplayCrosshair = TaggingDisplayCrosshair,
				HyperDomainAuthenticationData =
						HyperDomainAuthenticationDataDto.CreateFromModel(HyperDomainAuthenticationData),
				HyperNodeAuthenticationData = HyperNodeAuthenticationDataDto.CreateFromModel(HyperNodeAuthenticationData)
			};

			var customNodes = GetCustomNodes().Select(n => HyperNodeAuthenticationDataDto.CreateFromModel(n)).ToList();
			foreach(var node in customNodes)
			{
				settingsDto.CustomNodes.Add(node);
			}

			await _localStorageService.SetItemAsync("desiSettings", settingsDto);
		}
	}
}
