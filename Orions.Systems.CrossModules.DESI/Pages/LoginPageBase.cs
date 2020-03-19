﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.Desi.Core.ViewModels;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class LoginPageBase : DesiBaseComponent<AuthenticationViewModel>
	{
		protected ISettingsStorage SettingsStorage { get; set; }

		public LoginPageBase()
		{
		}

		public Task Initialize()
		{
			SettingsStorage = DependencyResolver.GetSettingsStorage();
			var authenticationSystem = DependencyResolver.GetAuthenticationSystem();
			var navigationService = DependencyResolver.GetNavigationService();

			Vm = new AuthenticationViewModel(authenticationSystem, navigationService)
			{
			};

			return Task.CompletedTask;
		}
	}
}
