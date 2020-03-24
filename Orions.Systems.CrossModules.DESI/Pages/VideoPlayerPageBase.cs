using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NReco.VideoConverter;

using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Media.Codecs.H264;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Core.ViewModels;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class VideoPlayerPageBase : DesiBaseComponent<AuthenticationViewModel>
	{
		private IHyperArgsSink _store;
		protected byte[] _payload;

		public VideoPlayerPageBase()
		{
		}

		public async Task Initialize()
		{
			var resp = await NetStore.ConnectAsync("http://root:@127.0.0.1:4580/Execute");
			_store = resp.Value;
			var args = new RetrieveAssetArgs(new HyperAssetId(Guid.Parse("e39ee30f-de57-41d5-1355-df41a404d708")));
			var asset = await _store.ExecuteAsync(args);
			var fragmentArgs = new RetrieveFragmentArgs(asset.Id, asset.DefaultVideoTrack.Id, asset.DefaultVideoTrack.CurrentFragmentsArray[0].Id, true);
			var fragment1 = await _store.ExecuteAsync(fragmentArgs);
			var codecInfo = asset.DefaultVideoTrack.MetaData.Default.GetString(HyperTrackId.VideoCodecPrivateDataFieldName);

			var h264Payload = fragment1.SlicesArray.SelectMany((x, i) => i == 0 ? H264Utilities.H264AddCodecDataToPayload(x.Data, codecInfo) : x.Data).ToArray();

			var inputStream = new MemoryStream(h264Payload);

			var ffmpegConverter = new FFMpegConverter();
			var outputStream = new MemoryStream();
			var settings = new ConvertSettings
			{
				//CustomOutputArgs = "-c copy -f ismv"
				CustomOutputArgs = "-c copy -movflags frag_keyframe+empty_moov -f mp4",
				CustomInputArgs = "-framerate 30"
			};
			var task = ffmpegConverter.ConvertLiveMedia(inputStream, null, outputStream, null, settings);
			task.Start();
			await Task.Run(() => task.Wait());
			_payload = outputStream.ToArray();

			//File.WriteAllBytes("C://test.mp4", _payload);
		}
	}
}
