using System.Web.Mvc;
using Orchard.Gallery.Impl;
using Orchard.Gallery.Interfaces;

namespace Orchard.Gallery.Attributes {
    public class StoreLastVisitedPackageListAttribute : ActionFilterAttribute {
        private readonly IPackageVisitTracker _packageVisitTracker;

        // HACK: We had to do poor man's injection, since the attribute cannot do automatic constructor injection.
        public StoreLastVisitedPackageListAttribute() : this(new PackageVisitTracker()) { }

        public StoreLastVisitedPackageListAttribute(IPackageVisitTracker packageVisitTracker) {
            _packageVisitTracker = packageVisitTracker;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            _packageVisitTracker.StoreLastVisitedPackageList(filterContext.HttpContext);
            base.OnActionExecuting(filterContext);
        }
    }
}