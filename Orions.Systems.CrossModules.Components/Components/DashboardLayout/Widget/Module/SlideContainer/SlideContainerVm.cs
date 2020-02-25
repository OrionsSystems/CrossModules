using Microsoft.AspNetCore.Components.Web;

using Orions.Common;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(SlideContainerWidget))]
	public class SlideContainerVm : WidgetVm<SlideContainerWidget>
	{

		private Dictionary<string, bool> Mapping = new Dictionary<string, bool>();

		private string ActiveId { get; set; }

		public SlideContainerVm()
		{
		}

		public void InitMapping()
		{
			if (Widget.Data != null && Widget.Data.Count() > 0)
			{
				for (var i = 0; i < Widget.Data.Count(); i++)
				{
					var el = Widget.Data[i];
					var isActive = false;
					if (i == 0)
					{
						isActive = true;
						ActiveId = el.Id;
					}

					Mapping.Add(el.Id, isActive);
				}
			}
		}

		public void SetActiveElement(string elementId)
		{
			ActiveId = elementId;

			foreach (var key in Mapping.Keys.ToList())
			{
				if (key == elementId)
				{
					Mapping[key] = true;
					continue;
				}

				Mapping[key] = false;
			}
		}

		/// <summary>
		/// find next element if exisF
		/// </summary>
		public void OnClickRightArrow(MouseEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(ActiveId) ||
				!Mapping.ContainsKey(ActiveId)) return;

			var keys = Mapping.Keys.ToArray();
			var index = Array.IndexOf(keys, ActiveId);
			var nextIndex = index + 1;
			if (keys.Count() <= nextIndex) return;

			var nextActiveId = keys[nextIndex];

			SetActiveElement(nextActiveId);
		}

		/// <summary>
		/// Find previous element
		/// </summary>
		public void OnClickLeftArrow(MouseEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(ActiveId) ||
				!Mapping.ContainsKey(ActiveId)) return;

			var keys = Mapping.Keys.ToArray();
			if (keys.Count() <= 1) return;
			var index = Array.IndexOf(keys, ActiveId);
			var previoustIndex = index - 1;
			if (previoustIndex < 0) return;
			var nextActiveId = keys[previoustIndex];

			SetActiveElement(nextActiveId);
		}

		public bool IsActiveElement(string elementId)
		{
			if (!string.IsNullOrWhiteSpace(elementId) && Mapping.ContainsKey(elementId))
			{
				return Mapping[elementId];
			}

			return false;
		}

		public bool IsFirstElementActive()
		{
			if (!string.IsNullOrWhiteSpace(ActiveId) && Mapping.ContainsKey(ActiveId)) 
			{
				var keys = Mapping.Keys.ToArray();
				var index = Array.IndexOf(keys, ActiveId);
				return index == 0;
			}

			return false;
		}

		public bool IsLastElementActive()
		{
			if (!string.IsNullOrWhiteSpace(ActiveId) && Mapping.ContainsKey(ActiveId))
			{
				var keys = Mapping.Keys.ToArray();
				var index = Array.IndexOf(keys, ActiveId);
				return index == keys.Count() - 1;
			}

			return false;
		}
	}
}
