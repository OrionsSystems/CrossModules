using System.Drawing;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Services;
using Orions.Systems.Desi.Common.Tracking;

namespace Orions.Systems.CrossModules.Desi.Infrastructure
{
	public class TrackerFactory : ITrackerFactory
	{
		public async Task<IEmguCvTracker> CreateEmguCvTracker(byte[] imageData, Rectangle roi, EmguCvTrackingConfiguration.Algorithm algorithm)
		{
			var tracker = new EmguCvTracker();
			await tracker.Init(imageData, roi, algorithm);
			return tracker;
		}
	}
}
