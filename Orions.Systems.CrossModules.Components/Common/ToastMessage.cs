using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public class ToastMessage
	{
		public string Message { get; set; }

		public ToastMessageType Type { get; set; }

		public ToastMessage(string message) {
			Message = message;
		}

		public ToastMessage(string message, ToastMessageType type)
		{
			Message = message;
			Type = type;
		}
	}

	public enum ToastMessageType
	{ 
		Info,
		Warning,
		Success,
		Error
	}
}
