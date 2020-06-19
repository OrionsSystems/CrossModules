using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.SDK;
using Orions.Systems.CrossModules.Portal.Helpers;

using System;
using System.Drawing;

namespace Orions.Systems.CrossModules.Portal.Services
{
	public class GraphicsService : IGraphicsService
	{
		public IHeatmapRenderHelper HeatmapRender => new HeatmapRenderHelper();

		public GraphicsService()
		{
		}

		public Graphics FromImage(UniImage image, out int? width, out int? height)
		{
			width = null;
			height = null;

			using (var defaultBitmap = image?.AsBitmap())
			{
				if (defaultBitmap == null)
					return null;
				width = defaultBitmap.Width;
				height = defaultBitmap.Height;
				return Graphics.FromImage(defaultBitmap);
			}
		}

		public UniImage GetMask(UniImage image, HyperTagGeometry geometry, HyperTagGeometryMask geometryMask, bool IsExtractMode)
		{
			using (var defaultBitmap = image?.AsBitmap())
			{
				if (defaultBitmap == null)
					return null;

				using (var maskedBitmap = RasterHelper.Instance.RenderTagMask(defaultBitmap, geometry, geometryMask, IsExtractMode))
				{
					if (maskedBitmap == null)
						return null;
					var maskedImage = new UniImage();
					maskedImage.FromBitmap(maskedBitmap);

					return maskedImage;
				}
			}
		}

		public void SetDimensions(UniImage lastImage)
		{
			using (var bitmap = lastImage.AsBitmap())
			{
				lastImage.Width = bitmap.Width;
				lastImage.Height = bitmap.Height;
			}
		}
	}
}
