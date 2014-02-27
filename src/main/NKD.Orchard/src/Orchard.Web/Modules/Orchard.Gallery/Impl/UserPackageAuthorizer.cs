using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Localization;
using Orchard.Security;

namespace Orchard.Gallery.Impl {
    public class UserPackageAuthorizer : IUserPackageAuthorizer {
        private readonly IOrchardServices _orchardServices;
        private readonly IAuthorizationService _authorizationService;

        public Localizer T { get; set; }

        public UserPackageAuthorizer(IOrchardServices orchardServices, IAuthorizationService authorizationService) {
            _orchardServices = orchardServices;
            _authorizationService = authorizationService;

            T = NullLocalizer.Instance;
        }

        public bool AuthorizedToEditPackage(string packageId) {
            IUser currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser == null) {
                return false;
            }
            PackagePart packagePart = CreateTempPackagePart(packageId);
            return _authorizationService.TryCheckAccess(Permissions.ManagePackages, currentUser, packagePart);
        }

        private PackagePart CreateTempPackagePart(string packageId)
        {
            var packagePart = _orchardServices.ContentManager.New<PackagePart>("Package");
            packagePart.PackageID = packageId;
            return packagePart;
        }
    }
}