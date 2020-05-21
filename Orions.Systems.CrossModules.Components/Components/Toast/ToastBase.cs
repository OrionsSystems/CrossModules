using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Components;

using Syncfusion.EJ2.Blazor;
using Syncfusion.EJ2.Blazor.Notifications;

namespace Orions.Systems.CrossModules.Components
{
	public class ToastBase : BaseBlazorComponent<ToastVm>, IDisposable
	{
		protected EjsToast ToastComponent { get; set; }

		[Parameter]
		public double TimeOut { get; set; } = 3000;

		[Parameter]
		public bool ShowCloseButton { get; set; } = true;


		[Parameter]
		public bool NewestOnTop { get; set; } = true;

		private ToastMessageType ToastType { get; set; } = ToastMessageType.Info;


		public void Show(ToastMessage msg)
		{
			var toastModel = new ToastModel
			{
				Position = new ToastPositionModel
				{
					X = "Right",
					Y = "Top"
				},
				Content = msg.Message,
				TimeOut = TimeOut,
				CssClass = GetCssClass()
			};

			ToastComponent.Show(toastModel);
		}

		public void Hide()
		{
			ToastComponent.Hide();
		}

		public void ShowMessage(string message)
		{
			var toastModel = new ToastModel
			{
				Position = new ToastPositionModel
				{
					X = "Right",
					Y = "Top"
				},
				Content = message,
				TimeOut = TimeOut,
				CssClass = GetCssClass()
			};

			ToastComponent.Show(toastModel);
		}

		public void ShowWarning(string message)
		{
			this.ToastType = ToastMessageType.Warning;

			this.ShowMessage(message);
		}

		public void ShowSuccess(string message)
		{
			this.ToastType = ToastMessageType.Success;

			this.ShowMessage(message);
		}

		public void ShowInfo(string message)
		{
			this.ToastType = ToastMessageType.Info;

			this.ShowMessage(message);
		}

		public void ShowError(string message)
		{
			this.ToastType = ToastMessageType.Error;

			this.ShowMessage(message);
		}

		private string GetCssClass()
		{
			switch (ToastType)
			{
				case ToastMessageType.Warning:
					return "orions-toast-waring";
				case ToastMessageType.Success:
					return "orions-toast-success";
				default:
					return "";
			}
		}

		void IDisposable.Dispose()
		{
			ToastComponent?.Dispose();
		}
	}
}
