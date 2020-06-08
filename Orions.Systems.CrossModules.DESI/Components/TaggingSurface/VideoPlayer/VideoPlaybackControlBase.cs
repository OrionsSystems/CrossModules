using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Orions.Systems.CrossModules.Desi.Components.TaggingSurface.Model;
using Orions.Systems.CrossModules.Desi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orions.Systems.CrossModules.Components.Desi.Infrastructure;

namespace Orions.Systems.CrossModules.Desi.Components.TaggingSurface
{
	public class VideoPlaybackControlBase : BaseComponent
	{
		private DotNetObjectReference<VideoPlaybackControlBase> _componentReference;
		private List<Action> _afterRenderTasks = new List<Action>();
		protected string _elementId;

		[Parameter]
		public TimeSpan CurrentTimePosition { get; set; }

		[Parameter]
		public TimeSpan TotalDuration { get; set; }

		[Parameter]
		public EventCallback OnPlay { get; set; }

		[Parameter]
		public EventCallback OnPause { get; set; }

		[Parameter]
		public EventCallback<TimeSpan> OnPositionChanged { get; set; }

		[Parameter]
		public EventCallback<TimelineMarker> OnMarkerClicked { get; set; }

		[Parameter]
		public bool IsPlaying { get; set; }

		[Parameter]
		public double VolumeLevel { get; set; }

		[Parameter]
		public EventCallback<double> OnVolumeLevelChanged { get; set; }

		protected double TimePlayedWidthPercentrage 
		{
			get
			{
				var percentage = CurrentTimePosition.TotalMilliseconds / TotalDuration.TotalMilliseconds * 100;
				if (percentage < 0) percentage = 0;
				if (percentage > 100) percentage = 100;
				return percentage;
			}
		}

		protected double TimeToPlayWidthPercentrage
		{
			get
			{
				return 100 - TimePlayedWidthPercentrage;
			}
		}

		[Parameter]
		public double PlaybackSpeed { get; set; } = 1;
		protected double[] PlaybackSpeedOptions = new double[] { 0.25, 0.5, 1, 2, 4 };

		[Parameter]
		public EventCallback<double> OnPlaybackSpeedChanged { get; set; }

		[Parameter]

		public List<TimelineMarker> TimelineMarkers { get; set; }

		[Parameter]
		public List<TimelineMarker> TrackingSequenceStartMarkers { get; set; }

		[Parameter]
		public List<TimelineMarker> TrackingSequenceEndMarkers { get; set; }

		[Parameter]
		public List<TimelineMarker> IntermediateElementsMarkers { get; set; }

		[Parameter]
		public Tuple<TimelineMarker, TimelineMarker> CurrentSelectedTrackingSequenceBoundaries { get; set; }

		public VideoPlaybackControlBase()
		{
			_elementId = $"playback-control-{Guid.NewGuid()}";
			_componentReference = DotNetObjectReference.Create<VideoPlaybackControlBase>(this);
		}

		protected override void OnAfterRenderSafe(bool firstRender)
		{
			if (firstRender)
			{
				JSRuntime.InvokeVoidAsync("Orions.Player.playbackControl.init", _elementId, _componentReference);
			}

			lock (_afterRenderTasks)
			{
				foreach (var t in _afterRenderTasks)
				{
					t.Invoke();
				}
				_afterRenderTasks.Clear();
			}
		}

		public void TimeLineMarkerClicked(TimelineMarker marker)
		{
			OnMarkerClicked.InvokeAsync(marker);
		}

		[JSInvokable]
		public async Task TimelineClick(double percentegePosition)
		{
			CurrentTimePosition = TimeSpan.FromMilliseconds(TotalDuration.TotalMilliseconds / 100 * percentegePosition);

			OnPositionChanged.InvokeAsync(CurrentTimePosition);

			UpdateState();
		}

		public void PlayPauseClick()
		{
			if (IsPlaying)
			{
				this.OnPause.InvokeAsync(null);
			}
			else
			{
				this.OnPlay.InvokeAsync(null);
			}
		}
	}
}
