using System.Web.Mvc;

namespace Orchard.Gallery {
    public class HttpUnauthorizedAccessToPackageResult : HttpUnauthorizedResult {
        public HttpUnauthorizedAccessToPackageResult(string packageId)
            : base(string.Format("You do not have permission to modify the Package '{0}'.", packageId))
        { }
    }
}