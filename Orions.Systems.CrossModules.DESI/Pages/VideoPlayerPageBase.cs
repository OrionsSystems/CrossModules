using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
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
		private NetStore _store;
		private TaskPlaybackInfo _taskPlaybackInfo;
		private UniImage _pausedFrame;

		protected bool Paused { get; private set; } = false;
		protected byte[] PayLoad { get; private set; }
		protected string PausedFrameBase64 { get; private set; }

		public VideoPlayerPageBase()
		{
		}

		[Parameter]
		public TaskModel CurrentTask { get; set; }

		public async Task InitializeAsync()
		{
			if (CurrentTask == null || CurrentTask.HyperId.AssetId == null || CurrentTask.HyperId.TrackId == null || CurrentTask.HyperId.FragmentId == null)
				return;

			InitStore();

			var assetTuple = await InitTaskPlaybackAsync();

			await InitMP4PayloadAsync(assetTuple.track, assetTuple.fragment);

			//File.WriteAllBytes("C://test.mp4", _payload);
		}

		private void InitStore()
		{
			var netStoreProv = DependencyResolver.GetNetStoreProvider();
			_store = netStoreProv.CurrentNetStore;
		}

		private async Task<(HyperTrack track, HyperFragment fragment)> InitTaskPlaybackAsync()
		{
			var trackArgs = new RetrieveTrackArgs(CurrentTask.HyperId.AssetId.Value, CurrentTask.HyperId.TrackId.Value);
			var track = await _store.ExecuteAsync(trackArgs);

			var fragmentArgs = new RetrieveFragmentArgs(CurrentTask.HyperId.AssetId.Value, CurrentTask.HyperId.TrackId.Value, CurrentTask.HyperId.FragmentId.Value, true);
			var fragment = await _store.ExecuteAsync(fragmentArgs);

			_taskPlaybackInfo = new TaskPlaybackInfo(new HyperFragment[] { fragment }, CurrentTask.HyperId.AssetId.Value, CurrentTask.HyperId.TrackId.Value);

			return (track, fragment);
		}

		private async Task InitMP4PayloadAsync(HyperTrack track, HyperFragment fragment)
		{
			var codecInfo = track.MetaData.Default.GetString(HyperTrackId.VideoCodecPrivateDataFieldName);

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

			PayLoad = outputStream.ToArray();
		}

		[JSInvokable]
		public async void OnPauseAsync(double positionInSeconds)
		{
			var cacheService = DependencyResolver.GetFrameCacheService();

			var curTime = TimeSpan.FromSeconds(positionInSeconds);
			var hyperId = _taskPlaybackInfo.GetPositionHyperId(curTime);

			if (hyperId.SliceId == null)
				return;

			var framePayload = await cacheService.GetCachedFrameAsync(_store, hyperId, null);

			_pausedFrame = new UniImage(framePayload);
			PausedFrameBase64 = _pausedFrame.DataBase64Link;
			Paused = true;
		}

		[JSInvokable]
		public void OnPositionUpdate(double positionInSeconds)
		{
			// TODO
		}

		[JSInvokable]
		public void OnPlay()
		{
			PausedFrameBase64 = null;
			Paused = false;
		}
	}
}
