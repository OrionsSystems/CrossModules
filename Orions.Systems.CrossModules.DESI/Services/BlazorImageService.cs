using Orions.Desi.Forms.Core.Services;
using Orions.Systems.Desi.Common.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

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
			return Task.FromResult(new byte[10]);
			throw new NotImplementedException();
		}

		public Task<(int Width, int Height)> GetImageSize(byte[] imageBytes)
		{
			return Task.FromResult((100, 100));
			throw new NotImplementedException();
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
