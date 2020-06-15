using Orions.SDK;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Helpers
{
	public class PredefinedCustomColorHelper
	{
		static string NoneStatusColor = "#000000";
		
		static string ActiveStatusColor = "#C0DE14";
		static string InactiveStatusColor = "#CCCCCC";
		static string DefaultStatusColor = "#444444";
		static string LoadingStatusColor = "#FFC90E";

		static string SynchronizedStatusColor = "#7092BE"; // Blue
		static string SynchronizingStatusColor = "#A092BE"; // ???
		static string DeSynchronizingStatusColor = "#FA6464"; // Red
		static string StatusDefaultStatusColor = "black";
		static string StatusModifiedStatusColor = "red";

		public static string GetColor(PredefinedCustomColors color) 
		{
			var hexColor = String.Empty;

			switch (color)
			{
				case PredefinedCustomColors.None:
					hexColor = NoneStatusColor;
					break;
				case PredefinedCustomColors.Default:
					hexColor = DefaultStatusColor;
					break;
				case PredefinedCustomColors.Active:
					hexColor = ActiveStatusColor;
					break;
				case PredefinedCustomColors.Loading:
					hexColor = LoadingStatusColor;
					break;
				case PredefinedCustomColors.Inactive:
					hexColor = InactiveStatusColor;
					break;
				case PredefinedCustomColors.Synchronized:
					hexColor = SynchronizedStatusColor;
					break;
				case PredefinedCustomColors.Synchronizing:
					hexColor = SynchronizingStatusColor;
					break;
				case PredefinedCustomColors.DeSynchronizing:
					hexColor = DeSynchronizingStatusColor;
					break;
				case PredefinedCustomColors.StatusModified:
					hexColor = StatusModifiedStatusColor;
					break;
				case PredefinedCustomColors.StatusDefault:
					hexColor = StatusDefaultStatusColor;
					break;
				default:
					break;
			}
			return hexColor;
		}
	}
}
