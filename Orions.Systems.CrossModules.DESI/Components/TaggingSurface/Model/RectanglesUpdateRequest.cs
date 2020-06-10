using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface.Model
{
	public class RectanglesUpdateRequest
	{
		public List<Rectangle> Addings { get; set; } = new List<Rectangle>();
		public List<Rectangle> Updates { get; set; } = new List<Rectangle>();
		public List<Rectangle> Removals { get; set; } = new List<Rectangle>();
	}
}
