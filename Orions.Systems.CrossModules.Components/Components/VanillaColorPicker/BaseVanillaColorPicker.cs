using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class BaseVanillaColorPicker : BaseOrionsComponent
	{
		protected VanilaColorPickerConfig Config { get; set; } = new VanilaColorPickerConfig();

		[Parameter]
		public string Label { get; set; }

		/// <summary>
		/// Which element the picker should be attached to.
		/// </summary>
		[Parameter]
		public string ParentId { get { return Config.ParentId; } set { Config.ParentId = value; } }

		/// <summary>
		/// If the picker is used as a popup, where to place it relative to the parent. false to add the picker as a normal child element of the parent.
		/// </summary>
		[Parameter]
		public string Popup { get { return Config.Popup;  } set { Config.Popup = value;  } }

		/// <summary>
		/// Suffix of a custom "layout_..." CSS class to handle the overall arrangement of the picker elements.
		/// </summary>
		[Parameter]
		public string Layout { get { return Config.Layout;  } set { Config.Layout = value;  } }

		/// <summary>
		/// Whether to enable adjusting the alpha channel.
		/// </summary>
		[Parameter]
		public bool Alpha { get { return Config.Alpha;  } set { Config.Alpha = value;  } }

		/// <summary>
		/// Whether to show a text field for color value editing.
		/// </summary>
		[Parameter]
		public bool Editor { get { return Config.Editor;  } set { Config.Editor = value;  } }

		/// <summary>
		/// How to display the selected color in the text field (the text field still supports input in any format).
		/// </summary>
		[Parameter]
		public string EditorFormat { get { return Config.EditorFormat;  } set { Config.EditorFormat = value;  } }

		/// <summary>
		/// Whether to have a "Cancel" button which closes the popup.
		/// </summary>
		[Parameter]
		public bool CancelButton { get { return Config.CancelButton;  } set { Config.CancelButton = value;  } }

		/// <summary>
		/// Initial color for the picker.
		/// </summary>
		[Parameter]
		public string Color { get { return Config.Color; } set { Config.Color = value; } }

		protected override async Task OnFirstAfterRenderAsync()
		{
			if (string.IsNullOrWhiteSpace(ParentId)) ParentId = Id;

			await JsInterop.InvokeAsync<object>("Orions.VanillaColorPicker.init", new object[] { Config });
		}
	}
}
