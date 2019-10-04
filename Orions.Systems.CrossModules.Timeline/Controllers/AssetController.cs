using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using Orions.SDK.Utilities;
using Orions.XPlatform;

namespace Orions.Systems.CrossModules.Timeline.Controllers
{
	public class AssetController : SuperController
	{
		[ResponseCache(Duration = 100000)]
		public async Task<ActionResult> Asset_HlsHostById(
			string id)
		{
			var service = new AssetXService(TimelineSettings.NodeInfo);

			var filter = new AssetRequest(id)
			{
				ServerUri = ServerUri,
			};

			var viewModel =  await service.GetAsync(filter);

			return Json(new { Host = viewModel.HlsServerUri });
		}
	}
}
