using Orions.Desi.Forms.Core.Services;
using Orions.Systems.Desi.Common.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SkiaSharp;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Extensions;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class BlazorImageService : IImageService
	{
		public Task<byte[]> CreateImageFromPixelMap<TPixelMap, TPixel>(TPixelMap pixelMap, CreatePixelDelegate<TPixel> createPixelDelegate) where TPixelMap : PixelMap<TPixel>
		{
			throw new NotImplementedException();
		}

		public Task<byte[]> Crop(byte[] imageData, Rectangle cropRectangle)
		{
			throw new NotImplementedException();
		}

		public Task<byte[]> CropProportionally(byte[] imageData, RectangleF proportionalCropRect)
		{
			using (var skBitmap = SKBitmap.Decode(imageData))
			{
				var absRect = proportionalCropRect.GetAbsoluteRectangle(new Size(skBitmap.Width, skBitmap.Height));

				var image = SKImage.FromBitmap(skBitmap);
				var subset = image.Subset(SKRectI.Create((int)absRect.X, (int)absRect.Y, (int)absRect.Width, (int)absRect.Height));
				// encode the image
				var encodedData = subset.Encode(SKEncodedImageFormat.Jpeg, 100);
		
				return Task.FromResult(encodedData.ToArray());
			}
		}

		public Task<(int Width, int Height)> GetImageSize(byte[] imageBytes)
		{
			using (var skBitmap = SKBitmap.Decode(imageBytes))
			{
				return Task.FromResult((skBitmap.Width, skBitmap.Height));
			}
		}

		public Task<Color> GetMostCommonColor(byte[] imageBytes)
		{
			throw new NotImplementedException();
		}

		public Task<(byte R, byte G, byte B, byte A)> GetPixelColorByProportionalPosition(byte[] imageBytes, Point proportionalPosition)
		{
			throw new NotImplementedException();
		}

		public Task<byte[]> ReplacePixels(byte[] imageBytes, ReplacePixelsDelegate replaceDelegate)
		{
			throw new NotImplementedException();
		}

		public Task<byte[]> ResizeImage(byte[] imageBytes, int maxWidth, int maxHeight)
		{
			throw new NotImplementedException();
		}

		public Task<bool> SavePhotoAsync(byte[] data, string folder, string filename)
		{
			throw new NotImplementedException();
		}
	}
}
