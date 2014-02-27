using System.Web.Mvc;

namespace Orchard.Gallery.Extensions {
    public static class UrlHelperExtensions {
        public static string PackageForAdmin(this UrlHelper urlHelper, string packageSlug) {
            return urlHelper.Action("Item", "PackageAdmin", new { PackageSlug = packageSlug, area = "Orchard.Gallery" });
        }
        public static string ListPackagesForAdmin(this UrlHelper urlHelper) {
            return urlHelper.Action("List", "PackageAdmin", new { area = "Orchard.Gallery" });
        }
    }
}