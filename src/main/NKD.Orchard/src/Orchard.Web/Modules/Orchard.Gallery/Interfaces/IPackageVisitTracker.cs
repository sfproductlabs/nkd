using System;
using System.Web;

namespace Orchard.Gallery.Interfaces {
    public interface IPackageVisitTracker : IDependency {
        void StoreLastVisitedPackageList(HttpContextBase context);
        Uri RetrieveLastVisitedPackageList(HttpContextBase context);
        void StoreLastVisitedPackageDetailsLink(HttpContextBase context);
        Uri RetrieveLastVisitedPackageDetailsLink(HttpContextBase context);
    }
}