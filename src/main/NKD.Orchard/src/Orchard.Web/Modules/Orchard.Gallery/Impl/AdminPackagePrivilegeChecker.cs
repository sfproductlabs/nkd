using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class AdminPackagePrivilegeChecker : IAdminPackagePrivilegeChecker {
        private readonly IOrchardServices _services;
        private readonly IAuthorizationService _authorizationService;

        public AdminPackagePrivilegeChecker(IOrchardServices services, IAuthorizationService authorizationService) {
            _services = services;
            _authorizationService = authorizationService;
        }

        public bool UserCanManageAllPackages(IUser user) {
            return user != null && (IsSuperuser(user) || HasManagePackagesPermission(user));
        }

        public bool UserCanManageAllPackages(int userId) {
            IUser user = _services.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Id == userId).List().FirstOrDefault<UserPart>();
            return UserCanManageAllPackages(user);
        }

        private bool HasManagePackagesPermission(IUser user) {
            return _authorizationService.TryCheckAccess(Permissions.ManagePackages, user, null);
        }

        private bool IsSuperuser(IUser user) {
            return _services.WorkContext.CurrentSite.SuperUser.Equals(user.UserName, StringComparison.Ordinal);
        }
    }
}