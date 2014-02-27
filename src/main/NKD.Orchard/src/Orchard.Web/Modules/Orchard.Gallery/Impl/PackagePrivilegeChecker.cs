using JetBrains.Annotations;
using Orchard.Gallery.Interfaces;
using Orchard.Security;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class PackagePrivilegeChecker : IPackagePrivilegeChecker {
        private readonly IUserkeyPackageService _userkeyPackageService;
        private readonly IAdminPackagePrivilegeChecker _adminPackagePrivilegeChecker;

        public PackagePrivilegeChecker(IUserkeyPackageService userkeyPackageService, IAdminPackagePrivilegeChecker adminPackagePrivilegeChecker) {
            _userkeyPackageService = userkeyPackageService;
            _adminPackagePrivilegeChecker = adminPackagePrivilegeChecker;
        }

        public bool UserCanManagePackage(IUser user, string packageId) {
            return user != null && (_adminPackagePrivilegeChecker.UserCanManageAllPackages(user) || _userkeyPackageService.UserCanAccessPackage(packageId, user.Id));
        }
    }
}