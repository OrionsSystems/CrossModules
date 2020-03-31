using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using Orions.Systems.Desi.Common.Authentication;
using Orions.Systems.Desi.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Infrastructure
{
	public class BrowserLocalSettingsStorage : ISettingsStorage
	{
		private readonly ILocalStorageService _localStorageService;
		private readonly IConfiguration _appConfig;
		private AppSettingsDto _appSettingsDto;
		private HyperNodeAuthenticationInfo _hyperNodeAuthenticationData;
		private HyperDomainAuthenticationInfo _hyperDomainAuthenticationInfo;

		public BrowserLocalSettingsStorage(ILocalStorageService localStorageService, IConfiguration appConfig)
		{
			_localStorageService = localStorageService;
			_appConfig = appConfig;
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
					ConnectionString = _appConfig["DefaultDevModeConnectionString"] ?? HyperNodeAuthenticationInfo.DefaultConnectionString,
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

		public class AppSettingsDto
		{
			public int Id { get; set; }
			public string Token { get; set; }
			public bool IsStaySigned { get; set; }
			public bool IsDevModeEnabled { get; set; }
			public bool TaggingDisplayCrosshair { get; set; }
			public IList<HyperNodeAuthenticationDataDto> CustomNodes { get; } = new List<HyperNodeAuthenticationDataDto>();
			public HyperNodeAuthenticationDataDto HyperNodeAuthenticationData { get; set; }
			public HyperDomainAuthenticationDataDto HyperDomainAuthenticationData { get; set; }
		}

		public class HyperNodeAuthenticationDataDto
		{
			public string Id { get; set; }
			public string Alias { get; set; }
			public string ConnectionString { get; set; }

			public HyperNodeAuthenticationInfo CreateModel()
			{
				return new HyperNodeAuthenticationInfo(ConnectionString, Alias, Id);
			}

			public void UpdateFromModel(HyperNodeAuthenticationInfo model)
			{
				Alias = model.Alias;
				ConnectionString = model.ConnectionString;
			}

			public static HyperNodeAuthenticationDataDto CreateFromModel(HyperNodeAuthenticationInfo model)
			{
				return new HyperNodeAuthenticationDataDto
				{
					ConnectionString = model.ConnectionString,
					Alias = model.Alias,
					Id = model.Id
				};
			}
		}

		public class HyperDomainAuthenticationDataDto
		{
			public string Username { get; set; }
			public string Password { get; set; }

			public HyperDomainAuthenticationInfo CreateModel()
			{
				return new HyperDomainAuthenticationInfo(Username, new Password(Password));
			}

			public static HyperDomainAuthenticationDataDto CreateFromModel(HyperDomainAuthenticationInfo model)
			{
				return new HyperDomainAuthenticationDataDto
				{
					Username = model.Username,
					Password = model.Password.Value
				};
			}
		}
	}
}
