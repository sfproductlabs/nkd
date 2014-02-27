using System;
using System.Web;
using Orchard.Gallery.Interfaces;

namespace Orchard.Gallery.Impl {
    public class PackageVisitTracker : IPackageVisitTracker {
        private const string LAST_VISITED_PACKAGE_LIST = "LastVisitedPackageList";
        private const string LAST_VISITED_PACKAGE_DETAILS_LINK = "LastVisitedPackageDetailsLink";

        public void StoreLastVisitedPackageList(HttpContextBase context) {
            if (context.Session != null) {
                context.Session[LAST_VISITED_PACKAGE_LIST] = context.Request.Url;
            }
        }

        public Uri RetrieveLastVisitedPackageList(HttpContextBase context) {
            return context.Session != null ? context.Session[LAST_VISITED_PACKAGE_LIST] as Uri : null;
        }

        public void StoreLastVisitedPackageDetailsLink(HttpContextBase context) {
            if (context.Session != null) {
                context.Session[LAST_VISITED_PACKAGE_DETAILS_LINK] = context.Request.Url;
            }
        }

        public Uri RetrieveLastVisitedPackageDetailsLink(HttpContextBase context) {
            return context.Session != null ? context.Session[LAST_VISITED_PACKAGE_DETAILS_LINK] as Uri : null;
        }
    }
}