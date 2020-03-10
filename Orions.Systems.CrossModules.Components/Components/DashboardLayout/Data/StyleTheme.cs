using Orions.Common;

using System;
using System.Linq;

namespace Orions.Systems.CrossModules.Components
{
	public class StyleTheme : IdUnifiedBlob, IName, IGroup
	{
		[DocumentDescriptor]
		public string Name { get; set; } = "New Theme";

		[DocumentDescriptor]
		public string Group { get; set; }

		[DocumentDescriptor]
		public string Tag { get; set; }

		[HelpText("Apply css styles to the bottom of the page")]
		[UniBrowsable(UniBrowsableAttribute.EditTypes.MultiLineText)]
		public string Styles { get; set; }

		private string[] _palletes = new string[] { };

		[HelpText("Palette for the chart series separated by comma")]
		public string PaletteValues
		{
			get
			{
				return String.Join(", ", _palletes);
			}
			set
			{
				_palletes = value?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(it => it.Trim()).ToArray();
			}
		}

		[HelpText("Palette for the chart series.")]
		public string[] Palettes
		{
			get { return _palletes; }
			set { _palletes = value; }
		}


		public StyleTheme()
		{
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
