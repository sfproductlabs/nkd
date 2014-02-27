using Orchard.Security;

namespace Orchard.Gallery.Interfaces {
    public interface IAdminPackagePrivilegeChecker : IDependency {
        bool UserCanManageAllPackages(IUser user);
        bool UserCanManageAllPackages(int userId);
    }
}