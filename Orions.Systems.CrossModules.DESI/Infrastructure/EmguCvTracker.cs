using System;
using System.Drawing;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Tracking;
using Orions.Systems.Desi.Common.Tracking;

namespace Orions.Systems.CrossModules.Desi.Infrastructure
{
	public class EmguCvTracker : IEmguCvTracker
	{
		private Tracker _tracker;

		public Task Init(byte[] imageData, Rectangle roi, EmguCvTrackingConfiguration.Algorithm algorithm) => Task.Run(() =>
		{
			switch (algorithm)
			{
				case EmguCvTrackingConfiguration.Algorithm.Boosting:
					_tracker = new TrackerBoosting();
					break;
				case EmguCvTrackingConfiguration.Algorithm.CSRT:
					_tracker = new TrackerCSRT();
					break;
				case EmguCvTrackingConfiguration.Algorithm.GOTURN:
					_tracker = new TrackerGOTURN();
					break;
				case EmguCvTrackingConfiguration.Algorithm.KCF:
					_tracker = new TrackerKCF();
					break;
				//case EmguCvTrackingConfiguration.Algorithm.MedianFlow:
				//	_tracker = new TrackerMedianFlow();
				//	break;
				case EmguCvTrackingConfiguration.Algorithm.MIL:
					_tracker = new TrackerMIL();
					break;
				case EmguCvTrackingConfiguration.Algorithm.MOSSE:
					_tracker = new TrackerMOSSE();
					break;
				case EmguCvTrackingConfiguration.Algorithm.TLD:
					_tracker = new TrackerTLD();
					break;

				default:
					throw new NotImplementedException($"Tracker of type {algorithm} is not yet supported.");
			}

			using (var mat = CreateMat(imageData))
			{
				_tracker.Init(mat, roi);
			}
		});

		public Task<Rectangle?> Update(byte[] imageData) => Task.Run(() =>
		{
			if (_tracker == null)
			{
				throw new InvalidOperationException($"You have to call {nameof(Init)} before using tracking {nameof(Update)}.");
			}

			using (var mat = CreateMat(imageData))
			{
				if (_tracker.Update(mat, out var roi))
				{
					return Task.FromResult<Rectangle?>(roi);
				}
				else
				{
					return Task.FromResult<Rectangle?>(null);
				}
			}
		});

		public void Dispose()
		{
			_tracker.Dispose();
		}

		private Mat CreateMat(byte[] imageData)
		{
			var img = new Mat();
			CvInvoke.Imdecode(imageData, ImreadModes.AnyColor, img);

			return img;
		}
	}
}
