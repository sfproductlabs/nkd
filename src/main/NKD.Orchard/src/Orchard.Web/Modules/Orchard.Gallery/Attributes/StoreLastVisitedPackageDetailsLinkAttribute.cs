using System.Web.Mvc;
using Orchard.Gallery.Impl;
using Orchard.Gallery.Interfaces;

namespace Orchard.Gallery.Attributes {
    public class StoreLastVisitedPackageDetailsLinkAttribute : ActionFilterAttribute
    {
        private readonly IPackageVisitTracker _packageVisitTracker;

        // HACK: We had to do poor man's injection, since the attribute cannot do automatic constructor injection.
        public StoreLastVisitedPackageDetailsLinkAttribute() : this(new PackageVisitTracker()) { }

        public StoreLastVisitedPackageDetailsLinkAttribute(IPackageVisitTracker packageVisitTracker)
        {
            _packageVisitTracker = packageVisitTracker;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _packageVisitTracker.StoreLastVisitedPackageDetailsLink(filterContext.HttpContext);
            base.OnActionExecuting(filterContext);
        }
    }
}