using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Orions.Cloud.Common.Data;
using ISessionAspCore = Microsoft.AspNetCore.Http.ISession;

namespace Orions.Systems.CrossModules.Timeline.Controllers
{
	public abstract class SuperController : Controller
	{
		private static readonly MemoryCache _gridUtillity = new MemoryCache(new MemoryCacheOptions());

		protected ISessionAspCore Session => HttpContext.Session;

		protected static readonly string Issuer = "Orions.Systems.CrossModules.Timeline";

		protected string ServerUri { get; set; }

		protected string CompanyId { get; set; }

		protected string TagId { get; set; }

		protected const int MaxTasksThreshold = 10;

		protected User ContextUser
		{
			get
			{
				foreach (var claim in HttpContext.User.Claims)
				{
					if (claim.Type != ClaimTypes.UserData) continue;
					if (string.IsNullOrWhiteSpace(claim.Value)) continue;
					return JsonConvert.DeserializeObject<User>(claim.Value);
				}

				return null;
			}
		}

		protected string Authentication
		{
			get
			{
				foreach (var claim in HttpContext.User.Claims)
				{
					if (claim.Type != ClaimTypes.Authentication) continue;
					return claim.Value;
				}

				return null;
			}
		}

		public override async Task OnActionExecutionAsync(
			ActionExecutingContext context, ActionExecutionDelegate next)
		{
			ServerUri = GetParamFromRequest("serverUri")?.ToString();

			CompanyId = GetParamFromRequest("companyId")?.ToString();

			TagId = GetParamFromRequest("tagId")?.ToString();

			await base.OnActionExecutionAsync(context, next);
		}

		protected object GetParamFromRequest(string paramName)
		{
			if (Request.Query.ContainsKey(paramName))
			{
				return Request.Query[paramName];
			}
			else
			{
				var key = RouteData.Values.FirstOrDefault(i => i.Key.ToLower() == paramName.ToLower());
				if (key.Value != null) return key.Value;
			}

			return null;
		}
	}
}
