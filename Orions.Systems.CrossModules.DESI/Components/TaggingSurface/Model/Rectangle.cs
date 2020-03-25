using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface.Model
{
	public class Rectangle
	{
		public string Id { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }

		public override bool Equals(object other)
		{
			var otherRect = other as Rectangle;
			if (otherRect == null)
			{
				return false;
			}

			if (otherRect.Id != this.Id
				|| otherRect.X != this.X
				|| otherRect.Y != this.Y
				|| otherRect.Height != this.Height
				|| otherRect.Width != this.Width)
				return false;

			return true;
		}
	}
}
