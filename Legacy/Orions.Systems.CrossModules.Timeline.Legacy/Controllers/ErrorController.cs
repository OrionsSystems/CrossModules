using Microsoft.AspNetCore.Mvc;

namespace Orions.Systems.CrossModules.Timeline.Controllers
{
	public class ErrorController : SuperController
	{
		public ActionResult Http404()
		{
			return View("NotFoundError");
		}

		public ActionResult Unauthorized()
		{
			return View("UnauthorizedError");
		}
	}
}