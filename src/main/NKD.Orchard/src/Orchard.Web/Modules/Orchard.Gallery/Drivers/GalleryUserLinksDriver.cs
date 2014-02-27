using Orchard.ContentManagement.Drivers;
using Orchard.Gallery.Models;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Gallery.Drivers {
    public class GalleryUserLinksDriver : ContentPartDriver<GalleryUserLinksPart> {
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrchardServices _orchardServices;

        public GalleryUserLinksDriver(IAuthorizationService authorizationService, IOrchardServices orchardServices) {
            _authorizationService = authorizationService;
            _orchardServices = orchardServices;
        }

        protected override DriverResult Display(GalleryUserLinksPart part, string displayType, dynamic shapeHelper) {
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            var userName = currentUser.UserName;
            var canAccessDashboard = _authorizationService.TryCheckAccess(Permission.Named("AccessAdminPanel"), currentUser, part);

            return ContentShape("Parts_GalleryUserLinks", () => shapeHelper.Parts_GalleryUserLinks(UserName: userName, CanAccessDashboard: canAccessDashboard));
        }
    }
}