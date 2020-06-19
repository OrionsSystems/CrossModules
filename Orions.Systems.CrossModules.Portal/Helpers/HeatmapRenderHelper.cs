using Orions.SDK;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Helpers
{
	public class HeatmapRenderHelper : IHeatmapRenderHelper
	{
		public SKImage GenerateMaskFromValuesMatrix(uint[,] matrix, int width, int height, uint? overrideNormalizationMin, uint? overrideNormalizationMax)
		{
			throw new NotImplementedException();
		}

		public SKImage RenderSkia(IEnumerable<HeatPoint> points, int width, int height, ClassificationSettings[] settings, SKBlendMode blendMode = SKBlendMode.Plus)
		{
			throw new NotImplementedException();
		}

		public SKImage RenderToSkImage(IEnumerable<HeatPoint> points, int width, int height)
		{
			throw new NotImplementedException();
		}
	}
}
