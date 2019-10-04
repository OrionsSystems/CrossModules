using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Orions.Systems.CrossModules.Timeline.Controllers
{
	[AllowAnonymous]
	[ResponseCache(Duration = 86400)]
	public class PlaylistController : SuperController
	{
		private static readonly string FileStreamType = "application/x-mpegurl";
		private static readonly string VideoStreamType = "video/mp2t";
		private static readonly string AudioStreamType = "audio/aac";

		public async Task<IActionResult> Hls(string assetId, string trackId, string connection)
		{
			var hlsUrl = $"http://{connection}/hls/{assetId}/asset.m3u8";

			if (!string.IsNullOrWhiteSpace(trackId))
				hlsUrl = $"http://{connection}/hls/{assetId}/{trackId}/track.m3u8";

			using (var client = new WebClient())
			using (var stream = client.OpenRead(hlsUrl))
			using (var reader = new StreamReader(stream))
			{
				var result = await reader.ReadToEndAsync();

				if (string.IsNullOrWhiteSpace(result))
					throw new InvalidOperationException("The provided URL does not link to a well-formed M3U8 playlist.");

				var currentHost = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

				var lines = result.Split('\n');

				var firstLine = lines[0];
				if (firstLine != "#EXTM3U")
					throw new InvalidOperationException("The provided URL does not link to a well-formed M3U8 playlist.");

				var trackLine = false;
				var trackVOD = false;
				for (var i = 1; i < lines.Length; i++)
				{
					var line = lines[i];

					if (line.StartsWith("#"))
					{
						var lineData = line.Substring(1);

						var split = lineData.Split(':');

						var name = split[0];

						switch (name)
						{
							case "EXT-X-MEDIA":
								line = line.Replace(
									"URI=\"",
									$"URI=\"{currentHost}/playlist/hls/{connection}/{assetId}/");
								break;

							case "EXT-X-PLAYLIST-TYPE":
								trackVOD = true;
								break;

							case "EXT-X-STREAM-INF":
								trackLine = true;
								if (!line.Contains("CODECS"))
								{
									line += ",CODECS=\"avc1.66.30,mp4a.40.2\"";
								}
								break;
						}
					}
					else
					{
						if (trackLine)
						{
							trackLine = false;
							if (!string.IsNullOrWhiteSpace(line))
								line = $"{currentHost}/playlist/hls/{connection}/{assetId}/{line.Trim()}";
						}
						if (trackVOD)
						{
							line = line.Replace(
								$"http://{connection}/hls/{assetId}/",
								$"{currentHost}/playlist/{nameof(Track)}/hls/{connection}/{assetId}/");
						}
					}


					lines[i] = line;
				}

				return Content(string.Join('\n', lines), FileStreamType);
			}
		}

		public async Task<IActionResult> Track(string assetId, string trackId, string connection, string index)
		{
			var trackUrl = $"http://{connection}/hls/{assetId}/{trackId}/{index}";
			var client = new WebClient();
			var stream = await client.OpenReadTaskAsync(trackUrl);

			if (trackId.ToLower().Contains("video"))
			{
				return base.File(stream, VideoStreamType);
			}
			else if (trackId.ToLower().Contains("audio"))
			{
				return base.File(stream, AudioStreamType);
			}
			else
			{
				return base.File(stream, VideoStreamType);
			}
		}
	}
}