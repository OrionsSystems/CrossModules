using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.JSInterop;

using NReco.VideoConverter;

using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Media.Codecs.H264;
using Orions.Node.Common;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using Orions.Systems.Desi.Common.Models;
using Orions.Systems.Desi.Core.ViewModels;

namespace Orions.Systems.CrossModules.Desi.Pages
{
	public class VideoPlayerPageBase : DesiBaseComponent<AuthenticationViewModel>
	{
		private IHyperArgsSink _store;
		private TaskPlaybackInfo _taskPlaybackInfo;
		private UniImage _pausedFrame;

		protected bool Paused { get; set; } = false;
		protected byte[] _payload;
		protected string PausedFrameBase64 { get; set; }

		public VideoPlayerPageBase()
		{
		}

		public async Task InitializeAsync()
		{
			await InitStoreAsync();
			var assetTuple = await InitTaskPlaybackAsync();
			await InitMP4PayloadAsync(assetTuple.asset, assetTuple.track, assetTuple.fragment);

			//File.WriteAllBytes("C://test.mp4", _payload);
		}

		private async Task InitStoreAsync()
		{
			var resp = await NetStore.ConnectAsync("http://root:@127.0.0.1:4580/Execute");
			_store = resp.Value;
		}

		private async Task<(HyperAsset asset, HyperTrack track, HyperFragment fragment)> InitTaskPlaybackAsync()
		{
			var args = new RetrieveAssetArgs(new HyperAssetId(Guid.Parse("e39ee30f-de57-41d5-1355-df41a404d708")));
			var asset = await _store.ExecuteAsync(args);
			var fragmentArgs = new RetrieveFragmentArgs(asset.Id, asset.DefaultVideoTrack.Id, asset.DefaultVideoTrack.CurrentFragmentsArray[0].Id, true);
			var fragment1 = await _store.ExecuteAsync(fragmentArgs);

			_taskPlaybackInfo = new TaskPlaybackInfo(new HyperFragment[] { fragment1 }, asset.Id, asset.DefaultVideoTrack.Id);

			return (asset, asset.DefaultVideoTrack, fragment1);
		}

		private async Task InitMP4PayloadAsync(HyperAsset asset, HyperTrack track, HyperFragment fragment)
		{
			var codecInfo = asset.DefaultVideoTrack.MetaData.Default.GetString(HyperTrackId.VideoCodecPrivateDataFieldName);

			var h264Payload = fragment.SlicesArray.SelectMany((x, i) => i == 0 ? H264Utilities.H264AddCodecDataToPayload(x.Data, codecInfo) : x.Data).ToArray();

			var inputStream = new MemoryStream(h264Payload);

			var ffmpegConverter = new FFMpegConverter();
			var outputStream = new MemoryStream();
			var settings = new ConvertSettings
			{
				CustomOutputArgs = "-c copy -movflags frag_keyframe+empty_moov -f mp4",
				CustomInputArgs = "-framerate 30"
			};
			var task = ffmpegConverter.ConvertLiveMedia(inputStream, null, outputStream, null, settings);
			task.Start();
			await Task.Run(() => task.Wait());

			_payload = outputStream.ToArray();
		}

		[JSInvokable]
		public async void OnPauseAsync(double positionInSeconds)
		{
			var curTime = TimeSpan.FromSeconds(positionInSeconds);
			var hyperId = _taskPlaybackInfo.GetPositionHyperId(curTime);

			if (hyperId.SliceId == null)
				return;

			var args = new RetrieveFragmentFramesArgs
			{
				AssetId = hyperId.AssetId.Value,
				TrackId = hyperId.TrackId.Value,
				FragmentId = hyperId.FragmentId.Value,
				SliceIds = new HyperSliceId[] { hyperId.SliceId.Value }
			};
			var res = await _store.ExecuteAsyncThrows(args);
			var img = res[0].Image;
			_pausedFrame = img;

			PausedFrameBase64 = img.DataBase64Link;
			Paused = true;
		}

		[JSInvokable]
		public void OnPlay()
		{
			PausedFrameBase64 = null;
			Paused = false;
		}
	}
}
