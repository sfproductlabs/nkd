using System;
using System.Web.Mvc;
using Orchard.Gallery.RatingSynchronization;

namespace Orchard.Gallery.Controllers {
    public class RatingsUpdateAuthorizationController : Controller {
        private readonly INonceCache _nonceCache;

        public RatingsUpdateAuthorizationController(INonceCache nonceCache) {
            _nonceCache = nonceCache;
        }

        [HttpGet]
        public JsonResult AuthorizeUpdate(string nonce) {
            bool nonceMatches = false;
            Guid parsedGuid;

            if (Guid.TryParse(nonce, out parsedGuid) && _nonceCache.Nonce == nonce)
            {
                nonceMatches = true;
                _nonceCache.Nonce = null;
            }

            return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = nonceMatches };
        }
    }
}