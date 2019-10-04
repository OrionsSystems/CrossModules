using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orions.XPlatform;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Timeline
{
	public abstract class SuperPageModel : PageModel
	{
		public string ServerUri { get; set; }

		public string TagId { get; set; }

		public List<SelectListItem> Nodes { get; }

        //Have an input box for the page number
        public bool Input { get; set; }

        //Have numeric pager
        public bool Numeric { get; set; }

        //Display the current range of items
        public bool Info { get; set; }

        //Display the 'first', 'previous', 'next' and 'last' buttons
        public bool PreviousNext { get; set; }

        //Display the 'refresh' button
        public bool Refresh { get; set; }

        //Allow the user to change the page size
        public bool PageSizes { get; set; }

        //By default the grid will show the pager even when total amount of items in the DataSource is less than the pageSize.
        public bool AlwaysVisible { get; set; }

        public SuperPageModel()
		{
			Nodes = new List<SelectListItem>();

            AlwaysVisible = true;
            PreviousNext = true;
            Refresh = true;

		}

		public string Authentication()
		{
			foreach (var claim in HttpContext.User.Claims)
			{
				if (claim.Type != ClaimTypes.Authentication) continue;
				return claim.Value;
			}

			throw new AuthenticationException();
		}
	}
}


