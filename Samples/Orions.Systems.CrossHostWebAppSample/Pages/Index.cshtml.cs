using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossHostWebAppSample.Pages
{
	public class IndexModel : PageModel
	{
		public void OnGet()
		{
			var z = Request;
			if (z.Query.Any(it => it.Key == "request"))
			{
				var instructionQuery = z.Query.FirstOrDefault(it => it.Key == "request");

				string json = System.Uri.UnescapeDataString(instructionQuery.Value);
				var req = new CrossModuleVisualizationRequest();
				JsonHelper.Populate(json, req);
			}
		}
	}
}
