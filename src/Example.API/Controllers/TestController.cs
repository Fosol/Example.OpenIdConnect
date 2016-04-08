using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Example.API.Controllers
{
    [Route("[controller]")]
    public class TestController : Controller
    {
        [HttpGet("alive")]
        public IActionResult Alive()
        {
            return new ObjectResult("alive");
        }

        [Authorize]
        [HttpGet("authenticated")]
        public IActionResult Authenticated()
        {
            return new ObjectResult(new { Name = User.Identity.Name });
        }

        [Authorize, HttpGet, Route("message")]
        public IActionResult GetMessage()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return HttpBadRequest();
            }

            // Note: identity is the ClaimsIdentity representing the resource owner
            // and identity.Actor is the identity corresponding to the client
            // application the access token has been issued to (delegation).
            return Content(string.Format(
                CultureInfo.InvariantCulture,
                "{0} has been successfully authenticated via {1}",
                identity.Name, identity.Actor.Name));
        }
    }
}
