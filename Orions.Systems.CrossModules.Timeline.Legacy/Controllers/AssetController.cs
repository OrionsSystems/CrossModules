using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using Orions.SDK.Utilities;
using System.Collections.Generic;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Timeline.Controllers
{
	public class AssetController : SuperController
	{
		[ResponseCache(Duration = 100000)]
		public async Task<ActionResult> Asset_HlsHostById(
			string id)
		{
			var stores = new Dictionary<string, IHyperArgsSink> { { ServerUri, NetStore } };
			var utility = new AssetUtility(stores);

			var request = new AssetRequest(id)
			{
				ServerUri = ServerUri,
			};

			var viewModel =  await utility.GetAsync(request);

			return Json(new { Host = viewModel.HlsServerUri });
		}
	}
}
