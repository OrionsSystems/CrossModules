using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

using Orions.Cloud.Common.Utils;
using Orions.SDK.Utilities;

namespace Orions.Systems.CrossModules.Timeline.Controllers
{
	[AllowAnonymous]
	[ResponseCache(Duration = CacheInterval)]	
	public class ThumbnailController : SuperController
	{
		static string ContentType = "image/jpeg";
		static string cacheKeyFormat = "{0}-{1}-{2}-{3}-{4}";

		const int CacheInterval = 86400;

		private static readonly MemoryCache _imagesCache = 
			new MemoryCache(new MemoryCacheOptions());
		private static readonly MemoryCache _assetTrackCache =
			new MemoryCache(new MemoryCacheOptions());

		public async Task<IActionResult> Index(
			string assetId, 
			string serverUri, 
			int width = 80, 
			int height = 60, 
			ulong index = 0)
		{
			var utility = new AssetUtility(GetStores());

			var key = string.Format(cacheKeyFormat, assetId, serverUri, width, height, index).GetHashCode();
			var byteArray = _imagesCache.Get<byte[]>(key);
			if (byteArray != null)
			{
				using (var stream = new MemoryStream(byteArray))
					return base.File(stream.ToArray(), ContentType);
			}

			var trackId = _assetTrackCache.Get<string>(assetId);
			if (trackId == null)
			{
				var assetVm = await utility.GetAsync(new AssetRequest(assetId) { ServerUri = serverUri });
				var track = assetVm.HyperAsset.VideoTracks.FirstOrDefault();
				trackId = track?.Id.ToString() ?? "";
				if (!string.IsNullOrWhiteSpace(trackId))
				{
					_assetTrackCache.Set(assetId, trackId, TimeSpan.FromSeconds(CacheInterval));
				}
			}

			byte[] thumbnail = null;
			if (!string.IsNullOrWhiteSpace(trackId))
			{
				thumbnail = await utility.GetSliceImageDataAsync(new AssetSliceRequest()
				{
					AssetId = assetId,
					ServerUri = serverUri,
					Width = width,
					Height = height,
					FragmentId = index,
					TrackId = trackId
				});
			}

			if (thumbnail == null || thumbnail.Length == 0)
			{
				DisableResponseCache();
				return GetEmptyThumbnail(width, height);
			}

			var data = ImageUtility.ResizeImage(thumbnail, width, height);
			var mimeType = ImageUtility.GetMimeType(ImageFormat.Jpeg);

			_imagesCache.Set(key, data, TimeSpan.FromSeconds(CacheInterval));

			return base.File(data, mimeType);
		}

		public async Task<IActionResult> Preview(
			string assetId, 
			string serverUri, 
			int width = 80, 
			int height = 60, 
			ulong index = 0)
		{
			var utility = new AssetUtility(GetStores());

			var key = string.Format(cacheKeyFormat, assetId, serverUri, width, height, index).GetHashCode();

			var byteArray = _imagesCache.Get<byte[]>(key);
			if (byteArray != null)
			{
				using (var stream = new MemoryStream(byteArray))
					return base.File(stream.ToArray(), ContentType);
			}

			var trackId = _assetTrackCache.Get<string>(assetId);
			if (trackId == null)
			{
				var assetVm = await utility.GetAsync(new AssetRequest(assetId) { ServerUri = serverUri });
				var track = assetVm.HyperAsset.VideoTracks.FirstOrDefault();
				trackId = track?.Id.ToString() ?? "";
				if (!string.IsNullOrWhiteSpace(trackId))
				{
					_assetTrackCache.Set(assetId, trackId, TimeSpan.FromSeconds(CacheInterval));
				}
			}

			byte[] thumbnail = null;
			if (!string.IsNullOrWhiteSpace(trackId))
			{
				thumbnail = await utility.GetSliceImageDataAsync(new AssetSliceRequest()
				{
					AssetId = assetId,
					ServerUri = serverUri,
					Width = width,
					Height = height,
					FragmentId = index,
					TrackId = trackId
				});
			}

			if (thumbnail == null || thumbnail.Length == 0)
			{
				DisableResponseCache();
				return GetEmptyThumbnail(width, height);
			}

			var data = ImageUtility.ResizeImage(thumbnail, width, height);
			var mimeType = ImageUtility.GetMimeType(ImageFormat.Jpeg);

			_imagesCache.Set(key, data, TimeSpan.FromSeconds(CacheInterval));

			BackgroundTaskManager.Queue(async () => await CacheNextTenSlides(assetId, serverUri, width, height, index++));

			return base.File(data, mimeType);
		}

		private async Task CacheNextTenSlides(
			string assetId, 
			string serverUri, 
			int width = 80, 
			int height = 60, 
			ulong index = 0)
		{
			var utility = new AssetUtility(GetStores());

			for (var i = index; i < index; i++)
			{
				var key = string.Format(cacheKeyFormat, assetId, serverUri, width, height, i).GetHashCode();

				var byteArray = _imagesCache.Get<byte[]>(key);
				if (byteArray != null) continue;

				var thumbnail = await utility.GetSliceImageDataAsync(new AssetSliceRequest()
				{
					AssetId = assetId,
					ServerUri = serverUri,
					Width = width,
					Height = height,
					FragmentId = i,
				});

				if (thumbnail == null || thumbnail.Length == 0) continue;

				var data = ImageUtility.ResizeImage(thumbnail, width, height);

				_imagesCache.Set(key, data, TimeSpan.FromSeconds(CacheInterval));
			}
		}

		private void DisableResponseCache()
		{
			if (Response.Headers.TryGetValue("Cache-Control", out StringValues value))
			{
				Response.Headers.Remove("Cache-Control");
				Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
			}
			if (Response.Headers.TryGetValue("Pragma", out value))
			{
				Response.Headers.Remove("Pragma");
				Response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
			}
			if (Response.Headers.TryGetValue("Expires", out value))
			{
				Response.Headers.Remove("Expires");
				Response.Headers.Add("Expires", "0"); // Proxies.
			}
		}

		private FileContentResult GetEmptyThumbnail(
			int width, 
			int height)
		{
			var data = ImageUtility.GetSolidColorImageData(width, height, Color.Silver, ImageFormat.Jpeg);
			var mimeType = ImageUtility.GetMimeType(ImageFormat.Jpeg);

			return base.File(data, mimeType);
		}
	}
}
